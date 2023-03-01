using System.Collections.Generic;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Notifications
{
    public class PaymentStatusChangedEmailNotification : OrderEmailNotificationBase
    {
        public PaymentStatusChangedEmailNotification()
            : base(nameof(PaymentStatusChangedEmailNotification))
        {
        }

        public IList<OrderOperationStatusChangedEntry> Entries { get; set; }
    }
}
