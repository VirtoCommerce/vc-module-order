using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Platform.Core.Notifications;

namespace VirtoCommerce.OrderModule.Data.Notifications
{
    public class InvoiceEmailNotification : OrderEmailNotificationBase
    {
        public InvoiceEmailNotification(IEmailNotificationSendingGateway gateway)
            : base(gateway)
        {
        }

        /// <summary>
        /// For templates back compatibility
        /// </summary>
        [NotificationParameter("Order")]
        public CustomerOrder Order
        {
            get
            {
                return base.CustomerOrder;
            }
        }

    }
}