using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Extensions;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Data.BackgroundJobs;

namespace VirtoCommerce.OrdersModule.Data.Handlers
{
    public class IndexCustomerOrderChangedEventHandler : IEventHandler<OrderChangedEvent>
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IConfiguration _configuration;

        public IndexCustomerOrderChangedEventHandler(ISettingsManager settingsManager, IConfiguration configuration)
        {
            _settingsManager = settingsManager;
            _configuration = configuration;
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

            IndexingJobs.EnqueueIndexAndDeleteDocuments(indexEntries);
        }
    }
}
