using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class OrderPaymentRequest : IHasOuterId
    {
        public string OrderId { get; set; }

        public string PaymentId { get; set; }

        public string TransactionId { get; set; }

        public string OuterId { get; set; }

        public decimal? Amount { get; set; }
    }
}
