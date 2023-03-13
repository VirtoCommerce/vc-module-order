namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class CaptureOrderPaymentRequest : OrderPaymentRequest
    {
        // Provides information about the charge that customers see on their statements. If Seller provides this information, a payment provider can implement it.
        public string CaptureDetails { get; set; }
    }
}
