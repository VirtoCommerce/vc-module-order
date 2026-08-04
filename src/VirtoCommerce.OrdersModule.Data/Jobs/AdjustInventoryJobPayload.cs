using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.OrdersModule.Data.Jobs
{
    /// <summary>
    /// Payload of the background job that adjusts reserved stock for a changed order.
    /// </summary>
    public class AdjustInventoryJobPayload
    {
        /// <summary>The changed order entry whose reservations must be applied.</summary>
        public GenericChangedEntry<CustomerOrder> ChangedEntry { get; set; }
    }
}