using VirtoCommerce.Platform.Core.Notifications;

namespace VirtoCommerce.OrderModule.Data.Notifications
{
    public class CancelOrderEmailNotification : OrderEmailNotificationBase
    {
        public CancelOrderEmailNotification(IEmailNotificationSendingGateway gateway)
            : base(gateway)
        {
        }
    }
}
