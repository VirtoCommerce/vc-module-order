using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.OrdersModule.Data.Services;

public class PurchasedProductService(
    Func<IOrderRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    IEventPublisher eventPublisher)
    : CrudService<PurchasedProduct, PurchasedProductEntity, PurchasedProductChangingEvent, PurchasedProductChangedEvent>
        (repositoryFactory, platformMemoryCache, eventPublisher),
        IPurchasedProductService
{
    protected override Task<IList<PurchasedProductEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((IOrderRepository)repository).GetPurchasedProductsByIdsAsync(ids, responseGroup);
    }
}
