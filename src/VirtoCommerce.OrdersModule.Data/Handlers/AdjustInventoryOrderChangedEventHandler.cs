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
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.OrdersModule.Data.Handlers
{
    /// <summary>
    /// Adjust inventory for ordered items 
    /// </summary>
    public class AdjustInventoryOrderChangedEventHandler : IEventHandler<OrderChangedEvent>
    {
        private readonly IStoreService _storeService;
        private readonly ISettingsManager _settingsManager;
        private readonly IItemService _itemService;
        private readonly IInventoryReservationService _reservationService;
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
            IStoreService storeService,
            ISettingsManager settingsManager,
            IItemService itemService,
            IInventoryReservationService reservationService,
            ILogger<AdjustInventoryOrderChangedEventHandler> logger)
        {
            _storeService = storeService;
            _settingsManager = settingsManager;
            _itemService = itemService;
            _reservationService = reservationService;
            _logger = logger;
        }

        /// <summary>
        /// Handles the order changed event by queueing a Hangfire task that adjusts inventories.
        /// </summary>
        /// <param name="message">Order changed event to handle.</param>
        /// <returns>A task that allows to <see langword="await"/> this method.</returns>
        public virtual async Task Handle(OrderChangedEvent message)
        {
            if (!await _settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.OrderAdjustInventory))
            {
                return;
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
        }

        public virtual async Task ProcessInventoryChanges(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var reserveStockRequest = await GetReserveRequests(changedEntry);

            if (reserveStockRequest.Items.Any())
            {
                await _reservationService.ReserveAsync(reserveStockRequest);
            }

            var releaseStockRequest = await GetReleaseRequests(changedEntry);

            if (releaseStockRequest.Items.Any())
            {
                await _reservationService.ReleaseAsync(releaseStockRequest);
            }
        }

        protected virtual async Task<InventoryReserveRequest> GetReserveRequests(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var order = changedEntry.NewEntry;
            var oldItems = changedEntry.OldEntry.Items?.ToArray() ?? Array.Empty<LineItem>();
            var newItems = changedEntry.NewEntry.Items?.ToArray() ?? Array.Empty<LineItem>();

            var request = AbstractTypeFactory<InventoryReserveRequest>.TryCreateInstance();
            request.ParentId = order.Id;
            var requestItems = new List<InventoryReservationRequestItem>();

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

                var requestItem = AbstractTypeFactory<InventoryReservationRequestItem>.TryCreateInstance();
                requestItem.ItemType = typeof(LineItem).FullName;
                requestItem.ItemId = newItem.Id;
                requestItem.ProductId = newItem.ProductId;
                requestItem.Quantity = newItem.Quantity;
                requestItems.Add(requestItem);
            });

            request.Items = await FilterByTrackInventory(requestItems);

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

        protected virtual async Task<InventoryReleaseRequest> GetReleaseRequests(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var order = changedEntry.NewEntry;
            var oldItems = changedEntry.OldEntry.Items?.ToArray() ?? Array.Empty<LineItem>();
            var newItems = changedEntry.NewEntry.Items?.ToArray() ?? Array.Empty<LineItem>();

            var request = AbstractTypeFactory<InventoryReleaseRequest>.TryCreateInstance();
            request.ParentId = order.Id;
            var requestItems = new List<InventoryReservationRequestItem>();

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

                var requestItem = AbstractTypeFactory<InventoryReservationRequestItem>.TryCreateInstance();
                requestItem.ItemType = typeof(LineItem).FullName;
                requestItem.ItemId = newItem.Id;
                requestItem.ProductId = newItem.ProductId;
                requestItems.Add(requestItem);
            });

            request.Items = await FilterByTrackInventory(requestItems);

            return request;
        }

        protected virtual async Task<IList<InventoryReservationRequestItem>> FilterByTrackInventory(IList<InventoryReservationRequestItem> items)
        {
            var result = new List<InventoryReservationRequestItem>();

            if (items.Any())
            {
                var productIds = items.Select(x => x.ProductId).ToArray();
                var catalogProducts = await _itemService.GetNoCloneAsync(productIds, ItemResponseGroup.None.ToString());

                var trackInventoryProductIds = catalogProducts
                    .Where(x => x.TrackInventory.HasValue && x.TrackInventory.Value)
                    .Select(x => x.Id);

                result.AddRange(items.Where(x => trackInventoryProductIds.Contains(x.ProductId)));
            }

            return result;
        }

        protected virtual async Task<IList<string>> GetFulfillmentCenterIdsAsync(string storeId)
        {
            var store = await _storeService.GetNoCloneAsync(storeId);

            if (store == null)
            {
                return Array.Empty<string>();
            }

            var result = new List<string>(store.AdditionalFulfillmentCenterIds ?? Array.Empty<string>());

            if (!string.IsNullOrEmpty(store.MainFulfillmentCenterId))
            {
                result.Insert(0, store.MainFulfillmentCenterId);
            }

            return result;
        }
    }
}
