using System.Collections.Generic;
using VirtoCommerce.PaymentModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class Refund : OrderOperation
    {
        public override string ObjectType { get; set; } = typeof(Refund).FullName;
        [Auditable]
        public decimal Amount { get; set; }
        [Auditable]
        public RefundReasonCode ReasonCode { get; set; }
        [Auditable]
        public RefundStatus RefundStatus { get; set; }
        [Auditable]
        public string ReasonMessage { get; set; }
        [Auditable]
        public string RejectReasonMessage { get; set; }
        [Auditable]
        public string VendorId { get; set; }
        [Auditable]
        public string TransactionId { get; set; }

        public string CustomerOrderId { get; set; }

        public string PaymentId { get; set; }

        public virtual ICollection<RefundItem> Items { get; set; }
    }
}
