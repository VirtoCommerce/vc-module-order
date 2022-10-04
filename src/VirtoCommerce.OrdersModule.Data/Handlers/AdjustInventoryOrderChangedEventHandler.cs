using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
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
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.OrdersModule.Data.Handlers
{
    public class ProductInventoryChange
    {
        public string ProductId { get; set; }
        public string FulfillmentCenterId { get; set; }
        public int QuantityDelta { get; set; }
    }


    /// <summary>
    /// Adjust inventory for ordered items 
    /// </summary>
    public class AdjustInventoryOrderChangedEventHandler : IEventHandler<OrderChangedEvent>
    {
        private readonly IInventoryService _inventoryService;
        private readonly ISettingsManager _settingsManager;
        private readonly IStoreService _storeService;
        private readonly IItemService _itemService;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="inventoryService">Inventory service to use for adjusting inventories.</param>
        /// <param name="storeService">Implementation of store service.</param>
        /// <param name="settingsManager">Implementation of settings manager.</param>
        /// <param name="itemService">Implementation of item service</param>
        public AdjustInventoryOrderChangedEventHandler(IInventoryService inventoryService, IStoreService storeService, ISettingsManager settingsManager, IItemService itemService)
        {
            _inventoryService = inventoryService;
            _settingsManager = settingsManager;
            _storeService = storeService;
            _itemService = itemService;
        }

        /// <summary>
        /// Handles the order changed event by queueing a Hangfire task that adjusts inventories.
        /// </summary>
        /// <param name="message">Order changed event to handle.</param>
        /// <returns>A task that allows to <see langword="await"/> this method.</returns>
        public virtual async Task Handle(OrderChangedEvent message)
        {
            if (_settingsManager.GetValue(ModuleConstants.Settings.General.OrderAdjustInventory.Name, true))
            {
                foreach (var changedEntry in message.ChangedEntries)
                {
                    var customerOrder = changedEntry.OldEntry;
                    //Do not process prototypes
                    if (!customerOrder.IsPrototype)
                    {
                        var itemChanges = await GetProductInventoryChangesFor(changedEntry);
                        if (itemChanges.Any())
                        {
                            //Background task is used here to  prevent concurrent update conflicts that can be occur during applying of adjustments for same inventory object
                            BackgroundJob.Enqueue(() => TryAdjustOrderInventoryBackgroundJob(itemChanges));
                        }
                    }
                }
            }
        }

        [DisableConcurrentExecution(10)]
        // "DisableConcurrentExecutionAttribute" prevents to start simultaneous job payloads.
        // Should have short timeout, because this attribute implemented by following manner: newly started job falls into "processing" state immediately.
        // Then it tries to receive job lock during timeout. If the lock received, the job starts payload.
        // When the job is awaiting desired timeout for lock release, it stucks in "processing" anyway. (Therefore, you should not to set long timeouts (like 24*60*60), this will cause a lot of stucked jobs and performance degradation.)
        // Then, if timeout is over and the lock NOT acquired, the job falls into "scheduled" state (this is default fail-retry scenario).
        // Failed job goes to "Failed" state (by default) after retries exhausted.
        public async Task TryAdjustOrderInventoryBackgroundJob(ProductInventoryChange[] productInventoryChanges)
        {
            await TryAdjustOrderInventory(productInventoryChanges);
        }

        /// <summary>
        /// Forms a list of product inventory changes for inventory adjustment. This method is intended for unit-testing only,
        /// and there should be no need to call it from the production code.
        /// </summary>
        /// <param name="changedEntry">The entry that describes changes made to order.</param>
        /// <returns>Array of required product inventory changes.</returns>
        public virtual async Task<ProductInventoryChange[]> GetProductInventoryChangesFor(GenericChangedEntry<CustomerOrder> changedEntry)
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
                    itemChanges.Add(itemChange);
                }
            });


            var allLineItems = newLineItems.Concat(oldLineItems).ToList();
            foreach (var item in itemChanges)
            {
                var lineItem = allLineItems.FirstOrDefault(x => x.ProductId == item.ProductId);
                if (lineItem != null)
                {
                    item.FulfillmentCenterId = await GetFullfilmentCenterForLineItemAsync(lineItem, customerOrder.StoreId, customerOrderShipments);
                }

            }
            //Do not return unchanged records
            return await Task.FromResult(itemChanges.Where(x => x.QuantityDelta != 0).ToArray());
        }


        public virtual async Task TryAdjustOrderInventory(ProductInventoryChange[] productInventoryChanges)
        {
            var inventoryAdjustments = new HashSet<InventoryInfo>();

            var productIds = productInventoryChanges.Select(x => x.ProductId).Distinct().ToArray();
            var catalogProducts = await _itemService.GetByIdsAsync(productIds, ItemResponseGroup.None.ToString());
            var inventoryInfos = (await _inventoryService.GetProductsInventoryInfosAsync(productIds)).ToList();

            foreach (var inventoryChange in productInventoryChanges)
            {
                var fulfillmentCenterId = inventoryChange.FulfillmentCenterId;
                var productId = inventoryChange.ProductId;
                var quantityDelta = inventoryChange.QuantityDelta;

                var inventoryInfo = inventoryInfos.FirstOrDefault(x => x.FulfillmentCenterId == (fulfillmentCenterId ?? x.FulfillmentCenterId) && x.ProductId.EqualsInvariant(productId));
                if (inventoryInfo == null)
                {
                    continue;
                }

                var catalogProduct = catalogProducts.FirstOrDefault(x => x.Id.EqualsInvariant(productId));
                if (catalogProduct == null || !catalogProduct.TrackInventory.HasValue || !catalogProduct.TrackInventory.Value)
                {
                    continue;
                }

                // "quantityDelta" - the count of additional items that should be taken from the inventory.
                inventoryInfo.InStockQuantity = Math.Max(0, inventoryInfo.InStockQuantity - quantityDelta);

                inventoryAdjustments.Add(inventoryInfo);
            }

            if (inventoryAdjustments.Any())
            {
                await _inventoryService.SaveChangesAsync(inventoryAdjustments);
            }
        }

        [Obsolete("This method is not used anymore. Please use the TryAdjustOrderInventory(OrderLineItemChange[]) method instead.")]
        protected virtual async Task TryAdjustOrderInventory(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var customerOrder = changedEntry.OldEntry;
            //Skip prototypes
            if (customerOrder.IsPrototype)
                return;

            var origLineItems = new LineItem[] { };
            var changedLineItems = new LineItem[] { };

            if (changedEntry.EntryState == EntryState.Added)
            {
                changedLineItems = changedEntry.NewEntry.Items.ToArray();
            }
            else if (changedEntry.EntryState == EntryState.Deleted)
            {
                origLineItems = changedEntry.OldEntry.Items.ToArray();
            }
            else
            {
                origLineItems = changedEntry.OldEntry.Items.ToArray();
                changedLineItems = changedEntry.NewEntry.Items.ToArray();
            }

            var inventoryAdjustments = new HashSet<InventoryInfo>();
            //Load all inventories records for all changes and old order items
            var inventoryInfos = await _inventoryService.GetProductsInventoryInfosAsync(origLineItems.Select(x => x.ProductId).Concat(changedLineItems.Select(x => x.ProductId)).Distinct().ToArray());
            changedLineItems.CompareTo(origLineItems, EqualityComparer<LineItem>.Default, async (state, changed, orig) =>
            {
                await AdjustInventory(inventoryInfos, inventoryAdjustments, customerOrder, state, changed, orig);
            });
            //Save inventories adjustments
            if (inventoryAdjustments != null)
            {
                await _inventoryService.SaveChangesAsync(inventoryAdjustments);
            }
        }

        protected virtual async Task AdjustInventory(IEnumerable<InventoryInfo> inventories, HashSet<InventoryInfo> changedInventories, CustomerOrder order, EntryState action, LineItem changedLineItem, LineItem origLineItem)
        {
            var fulfillmentCenterId = await GetFullfilmentCenterForLineItemAsync(origLineItem, order.StoreId, order.Shipments?.ToArray());
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
        protected virtual async Task<string> GetFullfilmentCenterForLineItemAsync(LineItem lineItem, string orderStoreId, Shipment[] orderShipments)
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
                var inventoryInfos = (await _inventoryService.GetProductsInventoryInfosAsync(new[] { lineItem.ProductId })).ToList();
                var store = await _storeService.GetByIdAsync(orderStoreId, StoreResponseGroup.StoreFulfillmentCenters.ToString());
                if (store != null)
                {
                    result = inventoryInfos.FirstOrDefault(x => x.FulfillmentCenterId == store.MainFulfillmentCenterId && x.InStockQuantity > 0)?.FulfillmentCenterId;

                    if (string.IsNullOrEmpty(result))
                    {
                        var ffcIds = inventoryInfos.Where(x => x.InStockQuantity > 0).Select(x => x.FulfillmentCenterId);
                        result = store.AdditionalFulfillmentCenterIds.FirstOrDefault(x => ffcIds.Contains(x));
                    }
                }
            }

            return result;
        }
    }
}
