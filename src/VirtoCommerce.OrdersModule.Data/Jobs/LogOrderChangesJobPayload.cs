using VirtoCommerce.Platform.Core.ChangeLog;

namespace VirtoCommerce.OrdersModule.Data.Jobs
{
    /// <summary>
    /// Payload of the background job that persists the change-log entries collected for a changed order.
    /// </summary>
    public class LogOrderChangesJobPayload
    {
        /// <summary>The change-log entries to persist.</summary>
        public OperationLog[] OperationLogs { get; set; }
    }
}