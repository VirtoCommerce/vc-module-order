using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Data.Search.Indexed;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.BackgroundJobs;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using static VirtoCommerce.OrdersModule.Core.ModuleConstants.Settings.General;

namespace VirtoCommerce.OrdersModule.Data.Handlers
{
    public class IndexPurchasedProductsCustomerOrderChangedEventHandler : IEventHandler<OrderChangedEvent>
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IIndexingJobService _indexingJobService;
        private readonly IEnumerable<IndexDocumentConfiguration> _indexingConfigurations;

        public IndexPurchasedProductsCustomerOrderChangedEventHandler(
            ISettingsManager settingsManager,
            IIndexingJobService indexingJobService,
            IEnumerable<IndexDocumentConfiguration> indexingConfigurations)
        {
            _settingsManager = settingsManager;
            _indexingJobService = indexingJobService;
            _indexingConfigurations = indexingConfigurations;
        }

        public async Task Handle(OrderChangedEvent message)
        {
            if (!await _settingsManager.GetValueAsync<bool>(EventBasedPurchasedProductIndexation) &&
                !await _settingsManager.GetValueAsync<bool>(PurchasedProductIndexation))
            {
                return;
            }

            HashSet<string> productIds = [];
            foreach (var changedEntry in message.ChangedEntries.Where(x => x.NewEntry != null))
            {
                foreach (var productId in changedEntry.NewEntry.Items.Select(x => x.ProductId))
                {
                    productIds.Add(productId);
                };
            }

            var indexEntries = productIds
                .Select(x => new IndexEntry { Id = x, EntryState = EntryState.Modified, Type = KnownDocumentTypes.Product })
                .ToArray();

            _indexingJobService.EnqueueIndexAndDeleteDocuments(indexEntries, JobPriority.Normal,
                _indexingConfigurations.GetDocumentBuilders(KnownDocumentTypes.Product, typeof(PurchasedProductsChangesProvider)).ToList());
        }
    }
}
