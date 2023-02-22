using System.Collections.Generic;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Notifications
{
    public class ShipmentStatusChangedEmailNotification : OrderEmailNotificationBase
    {
        public ShipmentStatusChangedEmailNotification()
            : base(nameof(ShipmentStatusChangedEmailNotification))
        {
        }

        public IList<OrderOperationStatusChangedEntry> Entries { get; set; }
    }
}
