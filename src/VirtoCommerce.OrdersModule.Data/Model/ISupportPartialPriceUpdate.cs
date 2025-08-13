using System;
using System.Collections.Generic;

namespace VirtoCommerce.OrdersModule.Data.Model
{
    [Obsolete("This interface is deprecated and will be removed in future versions.", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
    public interface ISupportPartialPriceUpdate
    {
        void ResetPrices();
        IEnumerable<decimal> GetNonCalculatablePrices();
    }
}
