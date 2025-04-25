using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Services;

public interface IPurchasedProductsService
{
    Task<PurchasedProductsResult> GetPurchasedProductsAsync(PurchasedProductsRequest request);

    Task<IDictionary<string, IEnumerable<PurchasedProduct>>> GetGroupedPurchasedProductsAsync(PurchasedProductsGroupedRequest request);
}
