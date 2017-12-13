using VirtoCommerce.Platform.Core.Notifications;

namespace VirtoCommerce.OrderModule.Data.Notifications
{
    public class OrderCreateEmailNotification : OrderEmailNotificationBase
    {
        public OrderCreateEmailNotification(IEmailNotificationSendingGateway gateway)
            : base(gateway)
        {
        }
    }
}

