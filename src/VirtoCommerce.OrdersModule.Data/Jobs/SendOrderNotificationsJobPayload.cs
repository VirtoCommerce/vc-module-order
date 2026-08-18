using VirtoCommerce.OrdersModule.Data.Handlers;

namespace VirtoCommerce.OrdersModule.Data.Jobs
{
    /// <summary>
    /// Payload of the background job that sends the notifications a changed order triggered.
    /// </summary>
    public class SendOrderNotificationsJobPayload
    {
        /// <summary>The notifications to send, one argument per notification.</summary>
        public OrderNotificationJobArgument[] JobArguments { get; set; }
    }
}