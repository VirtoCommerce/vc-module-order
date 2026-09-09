using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Data.Handlers;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.OrdersModule.Data.Jobs
{
    /// <summary>
    /// Processes the refunds requested on a changed order, off the request thread.
    /// </summary>
    /// <remarks>
    /// Delegates to <see cref="RefundChangedOrderChangedEventHandler"/>, which still owns the logic and stays callable by
    /// background jobs enqueued by an earlier version that reference its method by name.
    /// </remarks>
    public class RefundChangedJobHandler(RefundChangedOrderChangedEventHandler eventHandler) : IBackgroundJobHandler<RefundChangedJobPayload>
    {
        public virtual Task Execute(RefundChangedJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return eventHandler.ProcessRefundChangesAsync(payload.JobArguments);
        }
    }
}