using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
        public class AdjustOrderInventoryJobArgs
        {
            public AdjustOrderInventoryJobArgs(EntryState entryState, string storeId, Shipment[] shipments,
                LineItem[] oldItems, LineItem[] newItems)
            {
                EntryState = entryState;
                StoreId = storeId;
                Shipments = shipments;
                OldItems = oldItems;
                NewItems = newItems;
            }

            public AdjustOrderInventoryJobArgs()
                : this(EntryState.Unchanged, string.Empty, new Shipment[0], new LineItem[0], new LineItem[0])
            {
            }

            public EntryState EntryState { get; set; }
            public string StoreId { get; set; }
            public Shipment[] Shipments { get; set; }
            public LineItem[] OldItems { get; set; }
            public LineItem[] NewItems { get; set; }
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

                    var storeId = customerOrder.StoreId;
                    var shipments = customerOrder.Shipments?.ToArray();
                    var oldLineItems = changedEntry.OldEntry.Items?.ToArray() ?? new LineItem[0];
                    var newLineItems = changedEntry.NewEntry.Items?.ToArray() ?? new LineItem[0];
                    var args = new AdjustOrderInventoryJobArgs(changedEntry.EntryState, storeId, shipments, oldLineItems, newLineItems);

                    BackgroundJob.Enqueue(() => TryAdjustOrderInventoryBackgroundJob(args));
                }
            }
            return Task.CompletedTask;
        }

        [DisableConcurrentExecution(60 * 60 * 24)]
        public void TryAdjustOrderInventoryBackgroundJob(AdjustOrderInventoryJobArgs args)
        {
            TryAdjustOrderInventory(args.EntryState, args.StoreId, args.Shipments, args.OldItems, args.NewItems);
        }

        protected virtual void TryAdjustOrderInventory(EntryState entryState, string orderStoreId, Shipment[] orderShipments,
            LineItem[] oldLineItems, LineItem[] newLineItems)
        {
            var origLineItems = oldLineItems;
            var changedLineItems = newLineItems;

            if (entryState == EntryState.Added)
            {
                origLineItems = new LineItem[0];
            }
            else if (entryState == EntryState.Deleted)
            {
                changedLineItems = new LineItem[0];
            }

            var inventoryAdjustments = new HashSet<InventoryInfo>();
            //Load all inventories records for all changes and old order items
            var lineItemIds = origLineItems.Select(x => x.ProductId).Concat(changedLineItems.Select(x => x.ProductId)).Distinct().ToArray();
            var inventoryInfos = _inventoryService.GetProductsInventoryInfos(lineItemIds);
            changedLineItems.CompareTo(origLineItems, EqualityComparer<LineItem>.Default,
                (state, changed, orig) => { AdjustInventory(inventoryInfos, inventoryAdjustments, orderStoreId, orderShipments, state, changed, orig); });
            //Save inventories adjustments
            if (inventoryAdjustments != null)
            {
                _inventoryService.UpsertInventories(inventoryAdjustments);
            }
        }

        [Obsolete("This method is not used anymore. Please use the TryAdjustOrderInventory(EntryState, string, Shipment[], LineItem[], LineItem[]) method instead.")]
        protected virtual void TryAdjustOrderInventory(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var customerOrder = changedEntry.OldEntry;
            if (customerOrder.IsPrototype)
            {
                return;
            }

            var storeId = customerOrder.StoreId;
            var shipments = customerOrder.Shipments?.ToArray();
            var oldLineItems = changedEntry.OldEntry.Items?.ToArray() ?? new LineItem[0];
            var newLineItems = changedEntry.NewEntry.Items?.ToArray() ?? new LineItem[0];

            TryAdjustOrderInventory(changedEntry.EntryState, storeId, shipments, oldLineItems, newLineItems);
        }

        protected virtual void AdjustInventory(IEnumerable<InventoryInfo> inventories, HashSet<InventoryInfo> changedInventories,
            string orderStoreId, Shipment[] orderShipments, EntryState action, LineItem changedLineItem, LineItem origLineItem)
        {
            var fulfillmentCenterId = GetFullfilmentCenterForLineItem(origLineItem, orderStoreId, orderShipments);
            var inventoryInfo = inventories.Where(x => x.FulfillmentCenterId == (fulfillmentCenterId ?? x.FulfillmentCenterId))
                                           .FirstOrDefault(x => x.ProductId.EqualsInvariant(origLineItem.ProductId));
            if (inventoryInfo != null)
            {
                int delta;

                if (action == EntryState.Added)
                {
                    delta = -origLineItem.Quantity;
                }
                else if (action == EntryState.Deleted)
                {
                    delta = origLineItem.Quantity;
                }
                else
                {
                    delta = origLineItem.Quantity - changedLineItem.Quantity;
                }

                if (delta != 0)
                {
                    changedInventories.Add(inventoryInfo);
                    inventoryInfo.InStockQuantity += delta;
                    inventoryInfo.InStockQuantity = Math.Max(0, inventoryInfo.InStockQuantity);
                }
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
