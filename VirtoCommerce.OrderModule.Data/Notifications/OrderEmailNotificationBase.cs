using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Platform.Core.Notifications;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.OrderModule.Data.Notifications
{
    public class OrderEmailNotificationBase : EmailNotification
    {
        public OrderEmailNotificationBase(IEmailNotificationSendingGateway gateway) : base(gateway) { }

        [NotificationParameter("CustomerOrder")]
        public CustomerOrder CustomerOrder { get; set; }
    }
}
