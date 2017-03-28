using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Platform.Core.Notifications;

namespace VirtoCommerce.OrderModule.Data.Notifications
{
    public class Invoice : Notification
    {
        public Invoice(IEmailNotificationSendingGateway gateway)
            : base(gateway)
        {
        }

        [NotificationParameter("Order")]
        public CustomerOrder Order { get; set; }
    }
}