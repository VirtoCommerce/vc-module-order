using System;
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
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Data.BackgroundJobs;
using VirtoCommerce.SearchModule.Data.Services;

namespace VirtoCommerce.OrdersModule.Data.Handlers
{
    public class IndexCustomerOrderChangedEventHandler : IEventHandler<OrderChangedEvent>
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IConfiguration _configuration;
        private readonly IEnumerable<IndexDocumentConfiguration> _indexingConfigurations;

        public IndexCustomerOrderChangedEventHandler(ISettingsManager settingsManager, IConfiguration configuration, IEnumerable<IndexDocumentConfiguration> indexingConfigurations)
        {
            _settingsManager = settingsManager;
            _configuration = configuration;
            _indexingConfigurations = indexingConfigurations;
        }

        public async Task Handle(OrderChangedEvent message)
        {
            if (!_configuration.IsOrderFullTextSearchEnabled() ||
                !await _settingsManager.GetValueAsync(ModuleConstants.Settings.General.EventBasedIndexation.Name, false))
            {
                return;
            }

            var indexEntries = message?.ChangedEntries
                .Select(x => new IndexEntry { Id = x.OldEntry.Id, EntryState = x.EntryState, Type = ModuleConstants.OrderIndexDocumentType })
                .ToArray() ?? Array.Empty<IndexEntry>();

            IndexingJobs.EnqueueIndexAndDeleteDocuments(indexEntries,
                JobPriority.Normal, _indexingConfigurations.GetBuildersForProvider(typeof(CustomerOrderChangesProvider)).ToList());
        }
    }
}
