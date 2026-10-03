using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Data.Handlers;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.OrdersModule.Data.Jobs
{
    /// <summary>
    /// Persists the change-log entries collected for a changed order, off the request thread.
    /// </summary>
    /// <remarks>
    /// Delegates to <see cref="LogChangesOrderChangedEventHandler"/>, which still owns the logic and stays callable by
    /// background jobs enqueued by an earlier version that reference its method by name.
    /// </remarks>
    public class LogOrderChangesJobHandler(LogChangesOrderChangedEventHandler eventHandler) : IBackgroundJobHandler<LogOrderChangesJobPayload>
    {
        public virtual Task Execute(LogOrderChangesJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return eventHandler.TryToLogChangesBackgroundJob(payload.OperationLogs);
        }
    }
}