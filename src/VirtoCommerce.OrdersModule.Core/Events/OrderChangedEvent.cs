using System.Collections.Generic;
using Newtonsoft.Json;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.OrdersModule.Core.Events
{
    public class OrderChangedEvent : GenericChangedEntryEvent<CustomerOrder>
    {
        [JsonConstructor]
        public OrderChangedEvent(IEnumerable<GenericChangedEntry<CustomerOrder>> changedEntries)
            : base(changedEntries)
        {
        }

    }
}
