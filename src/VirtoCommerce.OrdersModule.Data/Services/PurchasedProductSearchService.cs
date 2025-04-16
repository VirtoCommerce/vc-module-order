using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Models;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.OrdersModule.Data.Services;

public class PurchasedProductSearchService(
    Func<IOrderRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    IPurchasedProductService crudService,
    IOptions<CrudOptions> crudOptions)
    : SearchService<PurchasedProductSearchCriteria, PurchasedProductSearchResult, PurchasedProduct, PurchasedProductEntity>
        (repositoryFactory, platformMemoryCache, crudService, crudOptions),
        IPurchasedProductSearchService
{
    protected override IQueryable<PurchasedProductEntity> BuildQuery(IRepository repository, PurchasedProductSearchCriteria criteria)
    {
        var query = ((IOrderRepository)repository).PurchasedProducts;

        if (!string.IsNullOrEmpty(criteria.UserId))
        {
            query = query.Where(x => x.UserId == criteria.UserId);
        }

        if (!criteria.ProductIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.ProductIds.Contains(x.ProductId));
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(PurchasedProductSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(PurchasedProductEntity.Id) },
            ];
        }

        return sortInfos;
    }
}
