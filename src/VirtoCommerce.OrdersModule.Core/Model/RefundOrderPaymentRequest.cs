namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class RefundOrderPaymentRequest : OrderPaymentRequest
    {
        public string ReasonCode { get; set; }

        public string ReasonMessage { get; set; }
    }
}
