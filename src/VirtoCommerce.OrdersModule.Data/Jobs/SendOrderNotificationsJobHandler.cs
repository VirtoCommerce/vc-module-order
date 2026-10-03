using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Data.Handlers;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.OrdersModule.Data.Jobs
{
    /// <summary>
    /// Sends the notifications a changed order triggered, off the request thread.
    /// </summary>
    /// <remarks>
    /// Delegates to <see cref="SendNotificationsOrderChangedEventHandler"/>, which still owns the logic and stays callable by
    /// background jobs enqueued by an earlier version that reference its method by name.
    /// </remarks>
    public class SendOrderNotificationsJobHandler(SendNotificationsOrderChangedEventHandler eventHandler) : IBackgroundJobHandler<SendOrderNotificationsJobPayload>
    {
        public virtual Task Execute(SendOrderNotificationsJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return eventHandler.TryToSendOrderNotificationsAsync(payload.JobArguments);
        }
    }
}