using System.Collections.Generic;
using VirtoCommerce.PaymentModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class Refund : OrderOperation
    {
        public override string ObjectType { get; set; } = typeof(Refund).FullName;

        public decimal Amount { get; set; }

        public RefundReasonCode ReasonCode { get; set; }

        public RefundStatus RefundStatus { get; set; }

        public string ReasonMessage { get; set; }

        public string RejectReasonMessage { get; set; }

        public string VendorId { get; set; }

        public string TransactionId { get; set; }

        public string CustomerOrderId { get; set; }

        public string PaymentId { get; set; }

        public virtual ICollection<RefundItem> Items { get; set; }
    }
}
