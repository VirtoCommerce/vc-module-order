namespace VirtoCommerce.OrdersModule.Core.Model
{
    public static class PaymentFlowErrorCodes
    {
        /// <summary>
        /// Invalid request errors arise when your request has invalid parameters or the payment provider doesn’t support the operation
        /// </summary>
        public const string InvalidRequestError = "INVALID_REQUEST_ERROR";

        /// <summary>
        /// The payment provider is currently unavailable. Please try again later or contact customer support if the issue persists.
        /// </summary>
        public const string PaymentProviderUnavailable = "PAYMENT_PROVIDER_UNAVAILABLE";

        /// <summary>
        /// Payment failed. Please try again later or contact customer support if the issue persists.
        /// </summary>
        public const string PaymentFailed = "PAYMENT_FAILED";
    }
}
