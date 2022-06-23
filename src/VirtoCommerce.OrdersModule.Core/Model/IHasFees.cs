using System.Collections.Generic;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public interface IHasFees
    {
        public ICollection<FeeDetail> FeeDetails { get; set; }
    }
}
