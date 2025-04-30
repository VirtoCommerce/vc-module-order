using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Data.Search.Indexed;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings.Events;
using static VirtoCommerce.OrdersModule.Core.ModuleConstants.Settings.General;

namespace VirtoCommerce.OrdersModule.Data.Handlers
{
    public class PurchasedProductIndexationSettingChangedEventHandler : IEventHandler<ObjectSettingChangedEvent>
    {
        private readonly PurchasedProductsIndexConfigurator _configurator;

        public PurchasedProductIndexationSettingChangedEventHandler(PurchasedProductsIndexConfigurator configurator)
        {
            _configurator = configurator;
        }

        public virtual async Task Handle(ObjectSettingChangedEvent message)
        {
            if (message.ChangedEntries.Any(x => (x.EntryState == EntryState.Modified || x.EntryState == EntryState.Added) &&
                                                 x.NewEntry.Name == PurchasedProductIndexation.Name))
            {
                await _configurator.Configure();
            }
        }
    }
}
