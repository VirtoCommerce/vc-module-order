using System.Collections.Generic;
using Newtonsoft.Json;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.OrdersModule.Core.Events
{
    public class PaymentChangedEvent : GenericChangedEntryEvent<PaymentIn>
    {
        [JsonConstructor]
        public PaymentChangedEvent(IEnumerable<GenericChangedEntry<PaymentIn>> changedEntries)
            : base(changedEntries)
        {
        }

    }
}
