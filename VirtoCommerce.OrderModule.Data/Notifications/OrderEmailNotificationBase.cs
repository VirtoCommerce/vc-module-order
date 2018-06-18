using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Platform.Core.Notifications;

namespace VirtoCommerce.OrderModule.Data.Notifications
{
    public abstract class OrderEmailNotificationBase : EmailNotification
    {
        protected OrderEmailNotificationBase(IEmailNotificationSendingGateway gateway) : base(gateway) { }

        [NotificationParameter("Customer Order")]
        public virtual CustomerOrder CustomerOrder { get; set; }
    }
}
