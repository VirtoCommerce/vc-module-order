using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Data.Handlers;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.OrdersModule.Data.Jobs
{
    /// <summary>
    /// Adjusts reserved stock for a changed order, off the request thread.
    /// </summary>
    /// <remarks>
    /// Delegates to <see cref="AdjustInventoryOrderChangedEventHandler"/>, which still owns the logic and stays callable by
    /// background jobs enqueued by an earlier version that reference its method by name.
    /// </remarks>
    public class AdjustInventoryJobHandler(AdjustInventoryOrderChangedEventHandler eventHandler) : IBackgroundJobHandler<AdjustInventoryJobPayload>
    {
        public virtual Task Execute(AdjustInventoryJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return eventHandler.ProcessInventoryChanges(payload.ToChangedEntry());
        }
    }
}