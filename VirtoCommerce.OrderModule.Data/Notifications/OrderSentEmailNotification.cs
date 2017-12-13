using VirtoCommerce.Platform.Core.Notifications;

namespace VirtoCommerce.OrderModule.Data.Notifications
{
    public class OrderSentEmailNotification : OrderEmailNotificationBase
    {
        public OrderSentEmailNotification(IEmailNotificationSendingGateway gateway)
            : base(gateway)
        {
        }
    }
}
