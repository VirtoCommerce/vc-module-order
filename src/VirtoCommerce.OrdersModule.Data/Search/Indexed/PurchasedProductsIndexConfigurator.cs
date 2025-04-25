using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Data.Search.Indexed;

public class PurchasedProductsIndexConfigurator
{
    private readonly ISettingsManager _settingsManager;
    private readonly IEnumerable<IndexDocumentConfiguration> _indexDocumentConfigurations;
    private readonly PurchasedProductsChangesProvider _purchasedProductsChangesProvider;
    private readonly PurchasedProductsDocumentBuilder _purchasedProductsDocumentBuilder;

    public PurchasedProductsIndexConfigurator(
        ISettingsManager settingsManager,
        IEnumerable<IndexDocumentConfiguration> indexDocumentConfigurations,
        PurchasedProductsChangesProvider purchasedProductsChangesProvider,
        PurchasedProductsDocumentBuilder purchasedProductsDocumentBuilder)
    {
        _settingsManager = settingsManager;
        _indexDocumentConfigurations = indexDocumentConfigurations;
        _purchasedProductsChangesProvider = purchasedProductsChangesProvider;
        _purchasedProductsDocumentBuilder = purchasedProductsDocumentBuilder;
    }

    public async Task Configure()
    {
        var productIndexingConfigurations = _indexDocumentConfigurations
            .Where(x => x.DocumentType == KnownDocumentTypes.Product)
            .ToList();

        if (productIndexingConfigurations.Count != 0)
        {
            var source = new IndexDocumentSource
            {
                ChangesProvider = _purchasedProductsChangesProvider,
                DocumentBuilder = _purchasedProductsDocumentBuilder,
            };

            foreach (var configuration in productIndexingConfigurations)
            {
                var puchasedProductsIndexationEnabled = await _settingsManager.GetValueAsync<bool>(Core.ModuleConstants.Settings.General.PurchasedProductIndexation);

                var existingSource = configuration.RelatedSources.FirstOrDefault(x => x.ChangesProvider is PurchasedProductsChangesProvider
                    && x.DocumentBuilder is PurchasedProductsDocumentBuilder);

                if (existingSource != null)
                {
                    configuration.RelatedSources?.Remove(existingSource);
                }

                if (puchasedProductsIndexationEnabled)
                {
                    configuration.RelatedSources ??= new List<IndexDocumentSource>();
                    configuration.RelatedSources.Add(source);
                }
            }
        }
    }
}
