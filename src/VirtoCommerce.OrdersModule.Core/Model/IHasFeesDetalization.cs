using System.Collections.Generic;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public interface IHasFeesDetalization
    {
        public ICollection<FeeDetail> FeeDetails { get; set; }
    }
}
