using System.Collections.Generic;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.OrdersModule.Core.Events
{
    public class ShipmentChangeEvent : GenericChangedEntryEvent<Shipment>
    {
        public ShipmentChangeEvent(IEnumerable<GenericChangedEntry<Shipment>> changedEntries)
           : base(changedEntries)
        {
        }
    }
}
