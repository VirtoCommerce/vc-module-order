using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Data.BackgroundJobs;

namespace VirtoCommerce.OrdersModule.Data.Handlers
{
    public class IndexCustomerOrderChangedEventHandler : IEventHandler<OrderChangedEvent>
    {
        private readonly ISettingsManager _settingsManager;

        public IndexCustomerOrderChangedEventHandler(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        public async Task Handle(OrderChangedEvent message)
        {
            if (!await _settingsManager.GetValueAsync(ModuleConstants.Settings.General.EventBasedIndexation.Name, false))
            {
                return;
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var indexEntries = message.ChangedEntries
                .Select(x => new IndexEntry { Id = x.OldEntry.Id, EntryState = x.EntryState, Type = KnownDocumentTypes.CustomerOrder })
                .ToArray();

            IndexingJobs.EnqueueIndexAndDeleteDocuments(indexEntries);
        }
    }
}
