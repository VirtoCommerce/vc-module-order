using System.Collections.Generic;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class PurchasedProductsRequest
    {
        public string StoreId { get; set; }

        public string UserId { get; set; }

        public IList<string> ProductIds { get; set; } = [];

        public int? ProductsLimit { get; set; }
    }
}
