using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.OrdersModule.Data.Services
{
    public class ShipmentService : CrudService<Shipment, ShipmentEntity, ShipmentChangeEvent, ShipmentChangedEvent>, IShipmentService
    {
        public ShipmentService(
            Func<IOrderRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher) :

            base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
        }

        protected override Task<IList<ShipmentEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
        {
            return ((IOrderRepository)repository).GetShipmentsByIdsAsync(ids);
        }

        protected override void ClearCache(IList<Shipment> models)
        {
            GenericSearchCachingRegion<CustomerOrder>.ExpireRegion();

            var orderIds = models.Select(x => x.CustomerOrderId).Distinct().ToArray();

            foreach (var id in orderIds)
            {
                GenericCachingRegion<CustomerOrder>.ExpireTokenForKey(id);
            }
        }
    }
}
