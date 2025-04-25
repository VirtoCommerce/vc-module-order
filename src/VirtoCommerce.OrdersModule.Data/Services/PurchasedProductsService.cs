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
public class PurchasedProductsService : IPurchasedProductsService
{
    private readonly Func<IOrderRepository> _repositoryFactory;
    private readonly IPlatformMemoryCache _platformMemoryCache;

    public PurchasedProductsService(Func<IOrderRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache)
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

    public async Task<IDictionary<string, IEnumerable<PurchasedProduct>>> GetGroupedPurchasedProductsAsync(PurchasedProductsGroupedRequest request)
    {
        var cacheKeyPrefix = CacheKey.With(GetType(), nameof(GetGroupedPurchasedProductsAsync));

        var models = await _platformMemoryCache.GetOrLoadByIdsAsync(cacheKeyPrefix, request.ProductIds,
            FindPurchasedProductsNoCacheAsync2,
            ConfigureCache);

        var result = models
            .Where(x => !x.Items.IsNullOrEmpty())
            .ToDictionary(x => x.Id, x => x.Items.Select(x => new PurchasedProduct
            {
                ProductId = x.Id,
                StoreId = x.StoreId,
                UserId = x.UserId
            }));

        return result;
    }

    protected virtual async Task<IList<PurchasedProductEntity>> FindPurchasedProductsNoCacheAsync(string storeId, string userId, IList<string> productIds)
    {
        using var repository = _repositoryFactory();

        var query = GetUserOrdersQuery(repository, storeId, userId);

        var result = await query
            .SelectMany(order => order.Items)
            .Where(lineItem => productIds.Contains(lineItem.ProductId))
            .Select(lineItem => lineItem.ProductId)
            .Distinct()
            .Select(x => new PurchasedProductEntity { Id = x, UserId = userId, StoreId = storeId, PurchasedBefore = true })
            .ToListAsync();

        result.AddRange(productIds.Except(result.Select(x => x.Id))
            .Select(x => new PurchasedProductEntity { Id = x, UserId = userId, StoreId = storeId, PurchasedBefore = false }));

        return result;
    }

    protected virtual async Task<IList<PurchasedProductGroup>> FindPurchasedProductsNoCacheAsync2(IList<string> productIds)
    {
        using var repository = _repositoryFactory();

        var query = GetAllOrdersQuery(repository);

        var result = await query
            .SelectMany(order => order.Items
                .Where(item => productIds.Contains(item.ProductId))
                .Select(item => new
                {
                    item.ProductId,
                    order.StoreId,
                    UserId = order.CustomerId,
                }))
            .Distinct()
            .GroupBy(x => x.ProductId)
            .Select(x => new PurchasedProductGroup
            {
                Id = x.Key,
                Items = x.Select(y => new PurchasedProductEntity
                {
                    Id = y.ProductId,
                    StoreId = y.StoreId,
                    UserId = y.UserId,
                }).ToArray()
            })
            .ToListAsync();

        // Add missing products
        var missingProducts = productIds.Except(result.SelectMany(x => x.Items).Select(x => x.Id))
            .Select(x => new PurchasedProductGroup
            {
                Id = x,
                Items = []
            })
            .ToList();

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

    protected virtual IQueryable<CustomerOrderEntity> GetAllOrdersQuery(IOrderRepository repository)
    {
        var query = repository.CustomerOrders;

        return query;
    }


    protected virtual void ConfigureCache(MemoryCacheEntryOptions options, string id, Entity entity)
    {
        var token = GenericSearchCachingRegion<CustomerOrder>.CreateChangeToken();
        options.AddExpirationToken(token);
    }

    protected class PurchasedProductGroup : Entity
    {
        public PurchasedProductEntity[] Items { get; set; } = [];
    }

    protected class PurchasedProductEntity : Entity
    {
        public string UserId { get; set; }
        public string StoreId { get; set; }
        public bool PurchasedBefore { get; set; }
    }
}
