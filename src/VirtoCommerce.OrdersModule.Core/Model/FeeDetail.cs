using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class FeeDetail : Entity, ICloneable
    {
        public string FeeId { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }

        public virtual object Clone()
        {
            return MemberwiseClone() as FeeDetail;
        }
    }
}
