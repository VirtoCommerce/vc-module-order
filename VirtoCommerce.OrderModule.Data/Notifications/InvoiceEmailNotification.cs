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
    }
}