using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class RefundItem : AuditableEntity, ICloneable
    {
        // The quantity to refund.
        public int Quantity { get; set; }

        public string LineItemId { get; set; }
        public LineItem LineItem { get; set; }

        public string RefundId { get; set; }

        public string OuterId { get; set; }

        public virtual object Clone()
        {
            var result = MemberwiseClone() as RefundItem;

            result.LineItem = LineItem?.Clone() as LineItem;

            return result;
        }
    }
}
