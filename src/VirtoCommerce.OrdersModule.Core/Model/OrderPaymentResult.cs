namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class OrderPaymentResult
    {
        public bool Succeeded { get; set; }

        /// <summary>
        /// ECommerce Flow Relaied Type: 
        /// INVALID_REQUEST_ERROR: Invalid request errors arise when your request has invalid parameters or the payment provider doesn’t support the operation
        /// PAYMENT_PROVIDER_UNAVAILABLE: The payment provider is currently unavailable. Please try again later or contact customer support if the issue persists.
        /// Payment Provider Errors:
        /// PAYMENT_FAILED: Payment capture failed. Please try again later or contact customer support if the issue persists.
        /// </summary>
        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }
    }
}
