using VirtoCommerce.Platform.Core.Notifications;

namespace VirtoCommerce.OrderModule.Data.Notifications
{
    public class OrderPaidEmailNotification : OrderEmailNotificationBase
    {
        public OrderPaidEmailNotification(IEmailNotificationSendingGateway gateway)
            : base(gateway)
        {
        }
    }
}
