using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Data.Handlers
{
    /// <summary>
    /// Adjust inventory for ordered items 
    /// </summary>
    public class AdjustInventoryOrderChangedEventHandler : IEventHandler<OrderChangedEvent>
    {
        private readonly ISettingsManager _settingsManager;
        private readonly ICrudService<Store> _storeService;
        private readonly IItemService _itemService;
        private readonly IReservationService _reservationService;
        private readonly ILogger<AdjustInventoryOrderChangedEventHandler> _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="storeService">Implementation of store service.</param>
        /// <param name="settingsManager">Implementation of settings manager.</param>
        /// <param name="itemService">Implementation of item service</param>
        /// <param name="reservationService">Implementation of service for reserve product stocks</param>
        /// <param name="logger">Logger</param>
        public AdjustInventoryOrderChangedEventHandler(
            ICrudService<Store> storeService,
            ISettingsManager settingsManager,
            IItemService itemService,
            IReservationService reservationService,
            ILogger<AdjustInventoryOrderChangedEventHandler> logger)
        {
            _settingsManager = settingsManager;
            _storeService = storeService;
            _itemService = itemService;
            _reservationService = reservationService;
            _logger = logger;
        }

        /// <summary>
        /// Handles the order changed event by queueing a Hangfire task that adjusts inventories.
        /// </summary>
        /// <param name="message">Order changed event to handle.</param>
        /// <returns>A task that allows to <see langword="await"/> this method.</returns>
        public virtual Task Handle(OrderChangedEvent message)
        {
            if (!_settingsManager.GetValue(ModuleConstants.Settings.General.OrderAdjustInventory.Name, true))
            {
                return Task.CompletedTask;
            }

            foreach (var changedEntry in message.ChangedEntries)
            {
                var customerOrder = changedEntry.OldEntry;
                //Do not process prototypes
                if (!customerOrder.IsPrototype)
                {
                    //Background task is used here to  prevent concurrent update conflicts that can be occur during applying of adjustments for same inventory object
                    BackgroundJob.Enqueue(() => ProcessInventoryChanges(changedEntry));
                }
            }

            return Task.CompletedTask;
        }

        public virtual async Task ProcessInventoryChanges(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var reserveStockRequest = await GetReserveRequests(changedEntry);
            var releaseStockRequest = await GetReleaseRequests(changedEntry);

            if (reserveStockRequest.Items.IsNullOrEmpty() && releaseStockRequest.Items.IsNullOrEmpty())
            {
                _logger.LogInformation("ProcessInventoryChanges: No transaction request, order - {Order}", changedEntry.NewEntry.Id);
                return;
            }

            if (!reserveStockRequest.Items.IsNullOrEmpty())
            {
                await _reservationService.ReserveStockAsync(reserveStockRequest);
            }

            if (!releaseStockRequest.Items.IsNullOrEmpty())
            {
                await _reservationService.ReleaseStockAsync(releaseStockRequest);
            }
        }

        protected virtual async Task<ReserveStockRequest> GetReserveRequests(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var order = changedEntry.NewEntry;
            var oldItems = changedEntry.OldEntry.Items?.ToArray() ?? Array.Empty<LineItem>();
            var newItems = changedEntry.NewEntry.Items?.ToArray() ?? Array.Empty<LineItem>();

            var request = AbstractTypeFactory<ReserveStockRequest>.TryCreateInstance();
            request.OuterType = typeof(LineItem).FullName;
            request.ParentId = order.Id;
            var requestItems = new List<StockRequestItem>();

            newItems.CompareTo(oldItems, EqualityComparer<LineItem>.Default, (state, newItem, oldItem) =>
            {
                if (changedEntry.EntryState != EntryState.Added && state != EntryState.Added)
                {
                    return;
                }

                if (newItem.Quantity == 0)
                {
                    return;
                }

                var requestItem = AbstractTypeFactory<StockRequestItem>.TryCreateInstance();
                requestItem.OuterId = newItem.Id;
                requestItem.ProductId = newItem.ProductId;
                requestItem.Quantity = newItem.Quantity;
                requestItems.Add(requestItem);
            });

            request.Items = await FilterByProducts(requestItems);

            if (request.Items.Any())
            {
                var fulfillmentCenterIds = await GetFulfillmentCenterIdsAsync(order.StoreId);
                if (!fulfillmentCenterIds.Any())
                {
                    _logger.LogInformation("GetReserveRequests: No fulfillment centers, store - {Store}, store - {Order}", order.StoreId, changedEntry.NewEntry.Id);
                }
                request.FulfillmentCenterIds = fulfillmentCenterIds;
            }

            return request;
        }

        protected virtual async Task<ReleaseStockRequest> GetReleaseRequests(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var order = changedEntry.NewEntry;
            var oldItems = changedEntry.OldEntry.Items?.ToArray() ?? Array.Empty<LineItem>();
            var newItems = changedEntry.NewEntry.Items?.ToArray() ?? Array.Empty<LineItem>();

            var request = AbstractTypeFactory<ReleaseStockRequest>.TryCreateInstance();
            request.OuterType = typeof(LineItem).FullName;
            request.ParentId = order.Id;
            var requestItems = new List<StockRequestItem>();

            newItems.CompareTo(oldItems, EqualityComparer<LineItem>.Default, (state, newItem, oldItem) =>
            {
                if (changedEntry.EntryState != EntryState.Deleted
                    && state != EntryState.Deleted
                    && (changedEntry.NewEntry.Status != ModuleConstants.CustomerOrderStatus.Cancelled ||
                        changedEntry.OldEntry == null ||
                        changedEntry.OldEntry.Status == ModuleConstants.CustomerOrderStatus.Cancelled))
                {
                    return;
                }

                if (oldItem.Quantity <= 0)
                {
                    return;
                }

                var requestItem = AbstractTypeFactory<StockRequestItem>.TryCreateInstance();
                requestItem.OuterId = newItem.Id;
                requestItem.ProductId = newItem.ProductId;
                requestItems.Add(requestItem);
            });

            request.Items = await FilterByProducts(requestItems);

            return request;
        }

        protected virtual async Task<List<StockRequestItem>> FilterByProducts(IList<StockRequestItem> items)
        {
            var result = new List<StockRequestItem>();

            if (items.Any())
            {
                var productIds = items.Select(x => x.ProductId).ToArray();
                var catalogProducts = await _itemService.GetByIdsAsync(productIds, ItemResponseGroup.None.ToString());
                var existingProductIds = catalogProducts
                    .Where(x => x.TrackInventory.HasValue && x.TrackInventory.Value)
                    .Select(x => x.Id);

                result.AddRange(items.Where(x => existingProductIds.Contains(x.ProductId)));
            }

            return result;
        }

        protected virtual async Task<IList<string>> GetFulfillmentCenterIdsAsync(string storeId)
        {
            var store = await _storeService.GetByIdAsync(storeId);

            return store == null
                ? new List<string>()
                : (new List<string> { store.MainFulfillmentCenterId }).Concat(store.AdditionalFulfillmentCenterIds).ToList();
        }
    }
}
