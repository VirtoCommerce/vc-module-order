using System.Collections.Generic;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class Capture : OrderOperation
    {
        public override string ObjectType { get; set; } = typeof(Capture).FullName;
        [Auditable]
        public decimal Amount { get; set; }
        [Auditable]
        public string VendorId { get; set; }
        [Auditable]
        public string TransactionId { get; set; }

        public string CustomerOrderId { get; set; }

        public string PaymentId { get; set; }

        public virtual ICollection<CaptureItem> Items { get; set; }
        [Auditable]
        public bool CloseTransaction { get; set; }
    }
}
