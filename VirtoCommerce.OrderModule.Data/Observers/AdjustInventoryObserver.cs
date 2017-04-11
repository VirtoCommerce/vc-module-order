using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Inventory.Model;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.Domain.Order.Events;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.OrderModule.Data.Observers
{
	public class AdjustInventoryObserver : IObserver<OrderChangedEvent>
	{
		private readonly IInventoryService _inventoryService;
	    private readonly ISettingsManager _settingsManager;

	    public AdjustInventoryObserver(IInventoryService inventoryService, ISettingsManager settingsManager)
	    {
	        _inventoryService = inventoryService;
	        _settingsManager = settingsManager;
	    }
		#region IObserver<CustomerOrder> Members

		public void OnCompleted()
		{
		}

		public void OnError(Exception error)
		{
		}

		public void OnNext(OrderChangedEvent value)
		{
		    if (_settingsManager.GetValue("Order.AdjustInventory", true) && !value.ModifiedOrder.IsPrototype)
		    {
		        var origStockOutOperations = new LineItem[] { };
		        var modifiedStockOutOperations = new LineItem[] { };

		        if (value.ChangeState == EntryState.Added)
		        {
		            modifiedStockOutOperations = value.ModifiedOrder.Items.ToArray();
		        }
		        else if (value.ChangeState == EntryState.Deleted)
		        {
		            origStockOutOperations = value.OrigOrder.Items.ToArray();
		        }
		        else
		        {
		            origStockOutOperations = value.OrigOrder.Items.ToArray();
		            modifiedStockOutOperations = value.ModifiedOrder.Items.ToArray();
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
		}


		#endregion

		private void AdjustInventory(IEnumerable<InventoryInfo> inventories, List<InventoryInfo> changedInventories, EntryState action, KeyValuePair<string, int> pairSource, KeyValuePair<string, int>? pairTarget = null)
		{
			var inventoryInfo = inventories.FirstOrDefault(x => x.ProductId == pairSource.Key);
			if (inventoryInfo != null)
			{
				var delta = 0;
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
