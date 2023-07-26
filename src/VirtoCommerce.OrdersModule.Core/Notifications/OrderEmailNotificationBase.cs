using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.NotificationsModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Notifications
{
    public abstract class OrderEmailNotificationBase : EmailNotification
    {
        protected OrderEmailNotificationBase(string type)
            : base(type)
        {
        }

        public virtual string CustomerOrderId { get; set; }
        public virtual CustomerOrder CustomerOrder { get; set; }
        public virtual Member Customer { get; set; }
    }
}
