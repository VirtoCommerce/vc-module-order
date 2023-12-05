namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class CaptureOrderPaymentRequest : OrderPaymentRequest
    {
        /// <summary>
        /// Provides information about the charge that customers see on their statements. If Seller provides this information, a payment provider can implement it.
        /// </summary>
        public string CaptureDetails { get; set; }

        /// <summary>
        /// Set to True to close a transaction, restricting future capture operations against this order; otherwise, set to False. By default, False.
        /// </summary>
        public bool CloseTransaction { get; set; }
    }
}
