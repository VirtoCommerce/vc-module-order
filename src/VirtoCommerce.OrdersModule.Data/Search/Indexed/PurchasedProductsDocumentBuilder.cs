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
    private readonly ICustomerOrderService _customerOrderService;
    private readonly IPurchasedProductsService _purchasedBeforeService;
    private readonly ISettingsManager _settingsManager;

    public PurchasedProductsDocumentBuilder(
        ICustomerOrderService customerOrderService,
        IPurchasedProductsService purchasedBeforeService,
        ISettingsManager settingsManager)
    {
        _customerOrderService = customerOrderService;
        _purchasedBeforeService = purchasedBeforeService;
        _settingsManager = settingsManager;
    }

    public async Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
    {
        var request = new PurchasedProductsGroupedRequest
        {
            ProductIds = documentIds,
        };

        var products = await _purchasedBeforeService.GetGroupedPurchasedProductsAsync(request);

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
