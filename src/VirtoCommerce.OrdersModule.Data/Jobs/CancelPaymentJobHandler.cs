using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Data.Handlers;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.OrdersModule.Data.Jobs
{
    /// <summary>
    /// Voids or refunds the payments a changed order asked to cancel, off the request thread.
    /// </summary>
    /// <remarks>
    /// Delegates to <see cref="CancelPaymentOrderChangedEventHandler"/>, which still owns the logic and stays callable by
    /// background jobs enqueued by an earlier version that reference its method by name.
    /// </remarks>
    public class CancelPaymentJobHandler(CancelPaymentOrderChangedEventHandler eventHandler) : IBackgroundJobHandler<CancelPaymentJobPayload>
    {
        public virtual Task Execute(CancelPaymentJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return eventHandler.TryToCancelOrderPaymentsAsync(payload.JobArguments);
        }
    }
}