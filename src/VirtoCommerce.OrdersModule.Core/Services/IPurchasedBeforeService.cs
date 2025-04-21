using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Services;

public interface IPurchasedBeforeService
{
    Task<PurchasedProductsResult> GetPurchasedProductsAsync(PurchasedProductsRequest request);
}
