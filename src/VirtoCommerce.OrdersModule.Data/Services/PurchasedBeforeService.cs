using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Data.Services;
public class PurchasedBeforeService : IPurchasedBeforeService
{
    private readonly Func<IOrderRepository> _repositoryFactory;
    private readonly IPlatformMemoryCache _platformMemoryCache;

    public PurchasedBeforeService(Func<IOrderRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache)
    {
        _repositoryFactory = repositoryFactory;
        _platformMemoryCache = platformMemoryCache;
    }

    public virtual async Task<PurchasedProductsResult> GetPurchasedProductsAsync(PurchasedProductsRequest request)
    {
        var cacheKeyPrefix = CacheKey.With(GetType(), nameof(GetPurchasedProductsAsync), request.StoreId, request.UserId);

        var models = await _platformMemoryCache.GetOrLoadByIdsAsync(cacheKeyPrefix, request.ProductIds,
            missingIds => FindPurchasedProductsNoCacheAsync(request.StoreId, request.UserId, missingIds),
            ConfigureCache);

        var result = new PurchasedProductsResult
        {
            ProductIds = models.Where(x => x.PurchasedBefore).Select(x => x.Id).ToList()
        };

        return result;
    }

    protected virtual async Task<IList<PurchasedBeforeEntity>> FindPurchasedProductsNoCacheAsync(string storeId, string userId, IList<string> productIds)
    {
        using var repository = _repositoryFactory();

        var query = GetUserOrdersQuery(repository, storeId, userId);

        var result = await query
            .SelectMany(order => order.Items)
            .Where(lineItem => productIds.Contains(lineItem.ProductId))
            .Select(lineItem => lineItem.ProductId)
            .Distinct()
            .Select(x => new PurchasedBeforeEntity { Id = x, UserId = userId, PurchasedBefore = true })
            .ToListAsync();

        result.AddRange(productIds.Except(result.Select(x => x.Id))
            .Select(x => new PurchasedBeforeEntity { Id = x, UserId = userId }));

        return result;
    }

    protected virtual IQueryable<CustomerOrderEntity> GetUserOrdersQuery(IOrderRepository repository, string storeId, string userId)
    {
        var query = repository.CustomerOrders.Where(order => order.StoreId == storeId);

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(x => x.CustomerId == userId);
        }

        return query;
    }

    protected virtual void ConfigureCache(MemoryCacheEntryOptions options, string id, PurchasedBeforeEntity entity)
    {
        var token = GenericSearchCachingRegion<CustomerOrder>.CreateChangeToken();
        options.AddExpirationToken(token);
    }

    protected class PurchasedBeforeEntity : Entity
    {
        public string UserId { get; set; }
        public bool PurchasedBefore { get; set; }
    }
}
