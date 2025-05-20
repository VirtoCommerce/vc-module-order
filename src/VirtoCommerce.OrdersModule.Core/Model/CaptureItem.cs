using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class CaptureItem : AuditableEntity, IHasOuterId, ICloneable
    {
        public int Quantity { get; set; }

        public string LineItemId { get; set; }
        public LineItem LineItem { get; set; }

        public string CaptureId { get; set; }

        public string OuterId { get; set; }

        public virtual object Clone()
        {
            var result = MemberwiseClone() as CaptureItem;

            result.LineItem = LineItem?.Clone() as LineItem;

            return result;
        }
    }
}
