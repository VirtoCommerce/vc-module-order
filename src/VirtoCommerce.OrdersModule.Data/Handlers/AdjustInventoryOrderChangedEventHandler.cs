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
            var customerOrder = changedEntry.NewEntry;
            var oldLineItems = changedEntry.OldEntry.Items?.ToArray() ?? Array.Empty<LineItem>();
            var newLineItems = changedEntry.NewEntry.Items?.ToArray() ?? Array.Empty<LineItem>();

            var reserveStockRequest = AbstractTypeFactory<ReserveStockRequest>.TryCreateInstance();
            reserveStockRequest.OuterType = typeof(LineItem).FullName;
            reserveStockRequest.ParentId = customerOrder.Id;
            var stockRequestItems = new List<StockRequestItem>();

            newLineItems.CompareTo(oldLineItems, EqualityComparer<LineItem>.Default, (state, changedItem, originalItem) =>
            {
                if (changedEntry.EntryState != EntryState.Added && state != EntryState.Added)
                {
                    return;
                }

                if (changedItem.Quantity == 0)
                {
                    return;
                }

                var stockRequestItem = AbstractTypeFactory<StockRequestItem>.TryCreateInstance();
                stockRequestItem.OuterId = changedItem.Id;
                stockRequestItem.ProductId = changedItem.ProductId;
                stockRequestItem.Quantity = changedItem.Quantity;
                stockRequestItems.Add(stockRequestItem);
            });

            reserveStockRequest.Items = await FilterByProducts(stockRequestItems);

            if (reserveStockRequest.Items.Any())
            {
                var fulfillmentCenterIds = await GetFulfillmentCenterIdsAsync(customerOrder.StoreId);
                if (!fulfillmentCenterIds.Any())
                {
                    _logger.LogInformation("GetReserveRequests: No fulfillment centers, store - {Store}, store - {Order}", customerOrder.StoreId, changedEntry.NewEntry.Id);
                }
                reserveStockRequest.FulfillmentCenterIds = fulfillmentCenterIds;
            }

            return reserveStockRequest;
        }

        protected virtual async Task<ReleaseStockRequest> GetReleaseRequests(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var customerOrder = changedEntry.NewEntry;
            var oldLineItems = changedEntry.OldEntry.Items?.ToArray() ?? Array.Empty<LineItem>();
            var newLineItems = changedEntry.NewEntry.Items?.ToArray() ?? Array.Empty<LineItem>();

            var releaseStockRequest = AbstractTypeFactory<ReleaseStockRequest>.TryCreateInstance();
            releaseStockRequest.OuterType = typeof(LineItem).FullName;
            releaseStockRequest.ParentId = customerOrder.Id;
            var stockRequestItems = new List<StockRequestItem>();

            newLineItems.CompareTo(oldLineItems, EqualityComparer<LineItem>.Default, (state, changedItem, originalItem) =>
            {
                if (changedEntry.EntryState != EntryState.Deleted
                    && state != EntryState.Deleted
                    && (changedEntry.NewEntry.Status != ModuleConstants.CustomerOrderStatus.Cancelled ||
                        changedEntry.OldEntry == null ||
                        changedEntry.OldEntry.Status == ModuleConstants.CustomerOrderStatus.Cancelled))
                {
                    return;
                }

                if (originalItem.Quantity <= 0)
                {
                    return;
                }

                var stockRequestItem = AbstractTypeFactory<StockRequestItem>.TryCreateInstance();
                stockRequestItem.OuterId = changedItem.Id;
                stockRequestItem.ProductId = changedItem.ProductId;
                stockRequestItems.Add(stockRequestItem);
            });

            releaseStockRequest.Items = await FilterByProducts(stockRequestItems);

            return releaseStockRequest;
        }

        private async Task<List<StockRequestItem>> FilterByProducts(IList<StockRequestItem> releaseStockRequests)
        {
            var result = new List<StockRequestItem>();

            if (releaseStockRequests.Any())
            {
                var productIds = releaseStockRequests.Select(x => x.ProductId).ToArray();
                var catalogProducts = await _itemService.GetByIdsAsync(productIds, ItemResponseGroup.None.ToString());

                result.AddRange(from releaseStockRequest
                                in releaseStockRequests
                                let product = catalogProducts
                                    .FirstOrDefault(x => x.Id == releaseStockRequest.ProductId &&
                                                         x.TrackInventory.HasValue &&
                                                         x.TrackInventory.Value)
                                where product != null
                                select releaseStockRequest);
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
