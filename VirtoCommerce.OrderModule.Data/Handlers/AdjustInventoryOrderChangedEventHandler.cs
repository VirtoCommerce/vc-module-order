using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Domain.Inventory.Model;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.Domain.Order.Events;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.OrderModule.Data.Handlers
{
    /// <summary>
    /// Adjust inventory for ordered items 
    /// </summary>
    public class AdjustInventoryOrderChangedEventHandler : IEventHandler<OrderChangedEvent>
    {
        public class ProductInventoryChange
        {
            public string ProductId { get; set; }
            public string FulfillmentCenterId { get; set; }
            public int QuantityDelta { get; set; }
        }

        private readonly IInventoryService _inventoryService;
        private readonly ISettingsManager _settingsManager;
        private readonly IStoreService _storeService;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="inventoryService">Inventory service to use for adjusting inventories.</param>
        /// <param name="storeService">Implementation of store service.</param>
        /// <param name="settingsManager">Implementation of settings manager.</param>
        public AdjustInventoryOrderChangedEventHandler(IInventoryService inventoryService, IStoreService storeService,
            ISettingsManager settingsManager)
        {
            _inventoryService = inventoryService;
            _settingsManager = settingsManager;
            _storeService = storeService;
        }


        /// <summary>
        /// Handles the order changed event by queueing a Hangfire task that adjusts inventories.
        /// </summary>
        /// <param name="message">Order changed event to handle.</param>
        /// <returns>A task that allows to <see langword="await"/> this method.</returns>
        public virtual Task Handle(OrderChangedEvent message)
        {
            if (_settingsManager.GetValue("Order.AdjustInventory", true))
            {
                foreach (var changedEntry in message.ChangedEntries)
                {
                    var customerOrder = changedEntry.OldEntry;
                    //Do not process prototypes
                    if (!customerOrder.IsPrototype)
                    {
                        var itemChanges = GetProductInventoryChangesFor(changedEntry);
                        if (itemChanges.Any())
                        {
                            //Background task is used here to  prevent concurrent update conflicts that can be occur during applying of adjustments for same inventory object
                            BackgroundJob.Enqueue(() => TryAdjustOrderInventoryBackgroundJob(itemChanges));
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }

        [DisableConcurrentExecution(60 * 60 * 24)]
        public void TryAdjustOrderInventoryBackgroundJob(ProductInventoryChange[] productInventoryChanges)
        {
            TryAdjustOrderInventory(productInventoryChanges);
        }

        protected virtual void TryAdjustOrderInventory(ProductInventoryChange[] productInventoryChanges)
        {
            var inventoryAdjustments = new HashSet<InventoryInfo>();
            //Load all inventories records for all changes and old order items
            var productIds = productInventoryChanges.Select(x => x.ProductId).Distinct().ToArray();
            var inventoryInfos = _inventoryService.GetProductsInventoryInfos(productIds);
            foreach (var productInventoryChange in productInventoryChanges)
            {
                var inventoryInfo = inventoryInfos.Where(x => x.FulfillmentCenterId == (productInventoryChange.FulfillmentCenterId ?? x.FulfillmentCenterId))
                                           .FirstOrDefault(x => x.ProductId.EqualsInvariant(productInventoryChange.ProductId));
                if (inventoryInfo != null)
                {
                    inventoryAdjustments.Add(inventoryInfo);

                    // NOTE: itemChange.QuantityDelta keeps the count of additional items that should be taken from the inventory.
                    //       That's why we subtract it from the current in-stock quantity instead of adding it.
                    inventoryInfo.InStockQuantity = Math.Max(0, inventoryInfo.InStockQuantity - productInventoryChange.QuantityDelta);
                }
            }
            if (inventoryAdjustments.Any())
            {
                //Save inventories adjustments
                _inventoryService.UpsertInventories(inventoryAdjustments);
            }
        }

        [Obsolete("This method is not used anymore. Please use the TryAdjustOrderInventory(OrderLineItemChange[]) method instead.")]
        protected virtual void TryAdjustOrderInventory(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var customerOrder = changedEntry.OldEntry;
            if (!customerOrder.IsPrototype)
            {
                var itemChanges = GetProductInventoryChangesFor(changedEntry);
                if (itemChanges.Any())
                {
                    TryAdjustOrderInventory(itemChanges);
                }
            }
        }

        /// <summary>
        /// Forms a list of product inventory changes for inventory adjustment. This method is intended for unit-testing only,
        /// and there should be no need to call it from the production code.
        /// </summary>
        /// <param name="changedEntry">The entry that describes changes made to order.</param>
        /// <returns>Array of required product inventory changes.</returns>
        public virtual ProductInventoryChange[] GetProductInventoryChangesFor(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var customerOrder = changedEntry.NewEntry;
            var customerOrderShipments = customerOrder.Shipments?.ToArray();

            var oldLineItems = changedEntry.OldEntry.Items?.ToArray() ?? Array.Empty<LineItem>();
            var newLineItems = changedEntry.NewEntry.Items?.ToArray() ?? Array.Empty<LineItem>();

            var itemChanges = new List<ProductInventoryChange>();
            newLineItems.CompareTo(oldLineItems, EqualityComparer<LineItem>.Default, (state, changedItem, originalItem) =>
            {
                var newQuantity = changedItem.Quantity;
                var oldQuantity = originalItem.Quantity;

                if (changedEntry.EntryState == EntryState.Added || state == EntryState.Added)
                {
                    oldQuantity = 0;
                }
                else if (changedEntry.EntryState == EntryState.Deleted || state == EntryState.Deleted)
                {
                    newQuantity = 0;
                }

                if (oldQuantity != newQuantity)
                {
                    var itemChange = AbstractTypeFactory<ProductInventoryChange>.TryCreateInstance();

                    itemChange.ProductId = changedItem.ProductId;
                    itemChange.QuantityDelta = newQuantity - oldQuantity;
                    itemChange.FulfillmentCenterId = GetFullfilmentCenterForLineItem(changedItem, customerOrder.StoreId, customerOrderShipments);
                    itemChanges.Add(itemChange);
                }
            });
            //Do not return unchanged records
            return itemChanges.Where(x => x.QuantityDelta != 0).ToArray();
        }

        /// <summary>
        /// Returns a fulfillment center identifier much suitable for given order line item
        /// </summary>
        /// <param name="lineItem"></param>
        /// <param name="orderStoreId"></param>
        /// <param name="orderShipments"></param>
        /// <returns></returns>
        protected virtual string GetFullfilmentCenterForLineItem(LineItem lineItem, string orderStoreId, Shipment[] orderShipments)
        {
            if (lineItem == null)
            {
                throw new ArgumentNullException(nameof(lineItem));
            }

            var result = lineItem.FulfillmentCenterId;

            if (string.IsNullOrEmpty(result))
            {
                //Try to find a concrete shipment for given line item 
                var shipment = orderShipments?.Where(x => x.Items != null)
                                              .FirstOrDefault(s => s.Items.Any(i => i.LineItemId == lineItem.Id));
                if (shipment == null)
                {
                    shipment = orderShipments?.FirstOrDefault();
                }
                result = shipment?.FulfillmentCenterId;
            }

            //Use a default fulfillment center defined for store
            if (string.IsNullOrEmpty(result))
            {
                result = _storeService.GetById(orderStoreId)?.MainFulfillmentCenterId;
            }
            return result;
        }
    }
}
