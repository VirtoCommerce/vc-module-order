using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Platform.Core.Notifications;

namespace VirtoCommerce.OrderModule.Data.Notifications
{
    public class OrderWorkflowNotification : OrderEmailNotificationBase
    {
        public OrderWorkflowNotification(IEmailNotificationSendingGateway gateway)
            : base(gateway)
        {
        }

        /// <summary>
        /// For templates back compatibility
        /// </summary>
        [NotificationParameter("Order")]
        public CustomerOrder Order => CustomerOrder;
    }
}
