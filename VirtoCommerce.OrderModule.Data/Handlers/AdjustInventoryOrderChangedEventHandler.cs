using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Domain.Inventory.Model;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.Domain.Order.Events;
using VirtoCommerce.Domain.Order.Model;
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
        private readonly IInventoryService _inventoryService;
        private readonly ISettingsManager _settingsManager;

        public AdjustInventoryOrderChangedEventHandler(IInventoryService inventoryService, ISettingsManager settingsManager)
        {
            _inventoryService = inventoryService;
            _settingsManager = settingsManager;
        }


        public virtual Task Handle(OrderChangedEvent message)
        {
            if (_settingsManager.GetValue("Order.AdjustInventory", true))
            {
                foreach (var changedEntry in message.ChangedEntries)
                {
                    TryAdjustOrderInventory(changedEntry);
                }
            }
            return Task.CompletedTask;
        }

        protected virtual void TryAdjustOrderInventory(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            //Skip prototypes
            if (changedEntry.OldEntry.IsPrototype)
                return;

            var origStockOutOperations = new LineItem[] { };
            var modifiedStockOutOperations = new LineItem[] { };

            if (changedEntry.EntryState == EntryState.Added)
            {
                modifiedStockOutOperations = changedEntry.NewEntry.Items.ToArray();
            }
            else if (changedEntry.EntryState == EntryState.Deleted)
            {
                origStockOutOperations = changedEntry.OldEntry.Items.ToArray();
            }
            else
            {
                origStockOutOperations = changedEntry.OldEntry.Items.ToArray();
                modifiedStockOutOperations = changedEntry.NewEntry.Items.ToArray();
            }

            var originalPositions = new ObservableCollection<KeyValuePair<string, int>>(origStockOutOperations.GroupBy(x => x.ProductId).Select(x => new KeyValuePair<string, int>(x.Key, x.Sum(y => y.Quantity))));
            var modifiedPositions = new ObservableCollection<KeyValuePair<string, int>>(modifiedStockOutOperations.GroupBy(x => x.ProductId).Select(x => new KeyValuePair<string, int>(x.Key, x.Sum(y => y.Quantity))));

            var changedInventoryInfos = new List<InventoryInfo>();

            var inventoryInfos = _inventoryService.GetProductsInventoryInfos(originalPositions.Select(x => x.Key).Concat(modifiedPositions.Select(x => x.Key)).Distinct().ToArray());

            var comparer = AnonymousComparer.Create((KeyValuePair<string, int> x) => x.Key);
            modifiedPositions.CompareTo(originalPositions, comparer, (state, source, target) => { AdjustInventory(inventoryInfos, changedInventoryInfos, state, source, target); });
            if (changedInventoryInfos.Any())
            {
                _inventoryService.UpsertInventories(changedInventoryInfos);
            }
        }

        protected virtual void AdjustInventory(IEnumerable<InventoryInfo> inventories, ICollection<InventoryInfo> changedInventories, EntryState action, KeyValuePair<string, int> pairSource, KeyValuePair<string, int>? pairTarget = null)
        {
            var inventoryInfo = inventories.FirstOrDefault(x => x.ProductId == pairSource.Key);
            if (inventoryInfo != null)
            {
                int delta;

                if (action == EntryState.Added)
                {
                    delta = -pairSource.Value;
                }
                else if (action == EntryState.Deleted)
                {
                    delta = pairSource.Value;
                }
                else
                {
                    delta = pairTarget.Value.Value - pairSource.Value;
                }

                if (delta != 0)
                {
                    inventoryInfo.InStockQuantity += delta;
                    changedInventories.Add(inventoryInfo);
                }
            }

        }
    }
}
