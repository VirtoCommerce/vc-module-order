using VirtoCommerce.OrdersModule.Data.Handlers;

namespace VirtoCommerce.OrdersModule.Data.Jobs
{
    /// <summary>
    /// Payload of the background job that processes the refunds requested on a changed order.
    /// </summary>
    public class RefundChangedJobPayload
    {
        /// <summary>The refunds to process, one argument per payment refund.</summary>
        public RefundChangedJobArgument[] JobArguments { get; set; }
    }
}