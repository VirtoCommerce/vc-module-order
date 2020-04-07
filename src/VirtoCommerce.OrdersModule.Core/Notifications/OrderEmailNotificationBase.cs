using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.NotificationsModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Notifications
{
    public abstract class OrderEmailNotificationBase : EmailNotification
    {
        public OrderEmailNotificationBase()
        {

        }

        public OrderEmailNotificationBase(string type) : base(type)
        {

        }

        public virtual CustomerOrder CustomerOrder { get; set; }
        public virtual Member Customer { get; set; }
    }
}
