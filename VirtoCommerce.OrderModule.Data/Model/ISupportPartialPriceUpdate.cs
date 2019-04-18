using System.Collections.Generic;

namespace VirtoCommerce.OrderModule.Data.Model
{
    public interface ISupportPartialPriceUpdate
    {
        void ResetPrices();
        IEnumerable<decimal> GetNonCalculatablePrices();
    }
}
