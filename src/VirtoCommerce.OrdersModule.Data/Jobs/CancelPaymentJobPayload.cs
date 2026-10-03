using VirtoCommerce.OrdersModule.Data.Handlers;

namespace VirtoCommerce.OrdersModule.Data.Jobs
{
    /// <summary>
    /// Payload of the background job that voids or refunds the payments a changed order asked to cancel.
    /// </summary>
    public class CancelPaymentJobPayload
    {
        /// <summary>The payments to cancel, one argument per order payment.</summary>
        public PaymentToCancelJobArgument[] JobArguments { get; set; }
    }
}