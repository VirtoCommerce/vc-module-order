using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using static VirtoCommerce.OrdersModule.Core.ModuleConstants;

namespace VirtoCommerce.OrdersModule.Data.Search.Indexed;
public class PurchasedProductsDocumentBuilder : IIndexDocumentBuilder
{
    private readonly IPurchasedProductsService _purchasedProductsService;

    public PurchasedProductsDocumentBuilder(
        IPurchasedProductsService purchasedBeforeService,
        ISettingsManager settingsManager)
    {
        _purchasedProductsService = purchasedBeforeService;
    }

    public async Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
    {
        var request = new PurchasedProductsGroupedRequest
        {
            ProductIds = documentIds,
        };

        var products = await _purchasedProductsService.GetGroupedPurchasedProductsAsync(request);

        var result = products
            .Select(g => CreateDocument(g.Key, g.Value))
            .ToArray();

        return result;
    }

    private static IndexDocument CreateDocument(string productId, IEnumerable<PurchasedProduct> purchasedProducts)
    {
        var document = new IndexDocument(productId);

        foreach (var storeGroup in purchasedProducts.GroupBy(x => x.StoreId))
        {
            var users = storeGroup.Select(x => x.UserId).ToArray();

            document.AddFilterableCollection($"{PurchasedProductDocumentPrefix}_{storeGroup.Key}", users);
        }

        return document;
    }
}
