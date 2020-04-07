using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Notifications
{
    public class InvoiceEmailNotification : OrderEmailNotificationBase
    {
        public InvoiceEmailNotification() : base(nameof(InvoiceEmailNotification))
        {

        }

        public CustomerOrder Order => CustomerOrder;
    }
}
