using System.Collections.Generic;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public enum CaptureStatus
    {
        Pending,
        Processed,
        Rejected,
    }

    public class Capture : OrderOperation
    {
        public override string ObjectType { get; set; } = typeof(Capture).FullName;

        public decimal Amount { get; set; }

        public string VendorId { get; set; }

        public string TransactionId { get; set; }

        public string CustomerOrderId { get; set; }

        public string PaymentId { get; set; }

        public virtual ICollection<CaptureItem> Items { get; set; }
    }
}
