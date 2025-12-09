using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Extensions;
using VirtoCommerce.OrdersModule.Data.Search.Indexed;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.BackgroundJobs;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Data.Handlers
{
    public class IndexCustomerOrderChangedEventHandler : IEventHandler<OrderChangedEvent>
    {
        protected ISettingsManager SettingsManager { get; }

        protected IConfiguration Configuration { get; }

        protected IIndexingJobService IndexingJobService { get; }

        protected IEnumerable<IndexDocumentConfiguration> IndexingConfigurations { get; }

        public IndexCustomerOrderChangedEventHandler(
            ISettingsManager settingsManager,
            IConfiguration configuration,
            IIndexingJobService indexingJobService,
            IEnumerable<IndexDocumentConfiguration> indexingConfigurations)
        {
            SettingsManager = settingsManager;
            Configuration = configuration;
            IndexingJobService = indexingJobService;
            IndexingConfigurations = indexingConfigurations;
        }

        public virtual async Task Handle(OrderChangedEvent message)
        {
            if (!await ShouldIndexAsync())
            {
                return;
            }

            await IndexOrdersAsync(message);
        }

        protected virtual async Task<bool> ShouldIndexAsync()
        {
            return Configuration.IsOrderFullTextSearchEnabled() &&
                   await SettingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.EventBasedIndexation);
        }

        protected virtual Task IndexOrdersAsync(OrderChangedEvent message)
        {
            var indexEntries = GetOrderIndexEntries(message);

            if (indexEntries.Length > 0)
            {
                var documentBuilders = IndexingConfigurations
                    .GetDocumentBuilders(ModuleConstants.OrderIndexDocumentType, typeof(CustomerOrderChangesProvider))
                    .ToList();

                IndexingJobService.EnqueueIndexAndDeleteDocuments(indexEntries, JobPriority.Normal, documentBuilders);
            }

            return Task.CompletedTask;
        }

        protected virtual IndexEntry[] GetOrderIndexEntries(OrderChangedEvent message)
        {
            return message?.ChangedEntries
                .Select(x => new IndexEntry
                {
                    Id = x.OldEntry.Id,
                    EntryState = x.EntryState,
                    Type = ModuleConstants.OrderIndexDocumentType,
                })
                .ToArray() ?? [];
        }
    }
}
