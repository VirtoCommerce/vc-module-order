using System.Collections.Generic;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.OrdersModule.Core.Events
{
    public class PaymentChangeEvent : GenericChangedEntryEvent<PaymentIn>
    {
        public PaymentChangeEvent(IEnumerable<GenericChangedEntry<PaymentIn>> changedEntries)
           : base(changedEntries)
        {
        }
    }
}
