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
        public class OrderLineItemChange
        {
            public string ProductId { get; set; }
            public string FulfillmentCenterId { get; set; }
            public int QuantityDelta { get; set; }
        }


        private readonly IInventoryService _inventoryService;
        private readonly ISettingsManager _settingsManager;
        private readonly IStoreService _storeService;

        public AdjustInventoryOrderChangedEventHandler(IInventoryService inventoryService, IStoreService storeService, ISettingsManager settingsManager)
        {
            _inventoryService = inventoryService;
            _settingsManager = settingsManager;
            _storeService = storeService;
        }


        public virtual Task Handle(OrderChangedEvent message)
        {
            if (_settingsManager.GetValue("Order.AdjustInventory", true))
            {
                foreach (var changedEntry in message.ChangedEntries)
                {
                    //Skip prototypes
                    var customerOrder = changedEntry.OldEntry;
                    if (customerOrder.IsPrototype)
                    {
                        continue;
                    }

                    var itemChanges = GetLineItemChangesFor(changedEntry);
                    BackgroundJob.Enqueue(() => TryAdjustOrderInventoryBackgroundJob(changedEntry.EntryState, itemChanges));
                }
            }
            return Task.CompletedTask;
        }

        [DisableConcurrentExecution(60 * 60 * 24)]
        public void TryAdjustOrderInventoryBackgroundJob(EntryState entryState, OrderLineItemChange[] orderLineItemChanges)
        {
            TryAdjustOrderInventory(entryState, orderLineItemChanges);
        }

        protected virtual void TryAdjustOrderInventory(EntryState entryState, OrderLineItemChange[] orderLineItemChanges)
        {
            var inventoryAdjustments = new HashSet<InventoryInfo>();
            //Load all inventories records for all changes and old order items
            var lineItemIds = orderLineItemChanges.Select(x => x.ProductId).Distinct().ToArray();
            var inventoryInfos = _inventoryService.GetProductsInventoryInfos(lineItemIds);
            foreach (var orderLineItemChange in orderLineItemChanges)
            {
                AdjustInventory(inventoryInfos, inventoryAdjustments, orderLineItemChange);
            }

            //Save inventories adjustments
            _inventoryService.UpsertInventories(inventoryAdjustments);
        }

        [Obsolete("This method is not used anymore. Please use the TryAdjustOrderInventory(EntryState, OrderLineItemChange[]) method instead.")]
        protected virtual void TryAdjustOrderInventory(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var customerOrder = changedEntry.OldEntry;
            if (customerOrder.IsPrototype)
            {
                return;
            }

            var itemChanges = GetLineItemChangesFor(changedEntry);
            TryAdjustOrderInventory(changedEntry.EntryState, itemChanges);
        }

        protected virtual OrderLineItemChange[] GetLineItemChangesFor(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var customerOrder = changedEntry.NewEntry;
            var customerOrderShipments = customerOrder.Shipments.ToArray();

            var oldLineItems = changedEntry.OldEntry.Items?.ToArray() ?? new LineItem[0];
            var newLineItems = changedEntry.NewEntry.Items?.ToArray() ?? new LineItem[0];

            var itemChanges = new List<OrderLineItemChange>();
            oldLineItems.CompareTo(newLineItems, EqualityComparer<LineItem>.Default, (state, originalItem, changedItem) =>
            {
                int oldQuantity = originalItem.Quantity;
                int newQuantity = changedItem.Quantity;

                if (state == EntryState.Added)
                {
                    oldQuantity = 0;
                }
                else if (state == EntryState.Deleted)
                {
                    newQuantity = 0;
                }

                if (oldQuantity != newQuantity)
                {
                    itemChanges.Add(new OrderLineItemChange
                    {
                        ProductId = changedItem.ProductId,
                        QuantityDelta = newQuantity - oldQuantity,
                        FulfillmentCenterId = GetFullfilmentCenterForLineItem(changedItem, customerOrder.StoreId, customerOrderShipments)
                    });
                }
            });

            return itemChanges.ToArray();
        }

        protected virtual void AdjustInventory(IEnumerable<InventoryInfo> inventories, HashSet<InventoryInfo> changedInventories, OrderLineItemChange itemChange)
        {
            var inventoryInfo = inventories.Where(x => x.FulfillmentCenterId == (itemChange.FulfillmentCenterId ?? x.FulfillmentCenterId))
                                           .FirstOrDefault(x => x.ProductId.EqualsInvariant(itemChange.ProductId));
            if (inventoryInfo != null)
            {
                changedInventories.Add(inventoryInfo);

                // NOTE: itemChange.QuantityDelta keeps the count of additional items that should be taken from the inventory.
                //       That's why we subtract it from the current in-stock quantity instead of adding it.
                inventoryInfo.InStockQuantity = Math.Max(0, inventoryInfo.InStockQuantity - itemChange.QuantityDelta);
            }
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
