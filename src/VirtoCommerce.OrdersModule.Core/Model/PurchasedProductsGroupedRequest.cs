using System.Collections.Generic;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class PurchasedProductsGroupedRequest
    {
        public IList<string> ProductIds { get; set; } = [];
    }
}
