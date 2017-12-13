using VirtoCommerce.Platform.Core.Notifications;

namespace VirtoCommerce.OrderModule.Data.Notifications
{
    public class NewOrderStatusEmailNotification : OrderEmailNotificationBase
    {
        public NewOrderStatusEmailNotification(IEmailNotificationSendingGateway gateway)
            : base(gateway)
        {
        }

        [NotificationParameter("Old order status")]
        public string OldStatus { get; set; }

        [NotificationParameter("New order status")]
        public string NewStatus { get; set; }
    }
}
