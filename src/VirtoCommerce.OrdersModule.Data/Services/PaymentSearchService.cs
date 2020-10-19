using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Caching;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.OrdersModule.Data.Services
{
    public class PaymentSearchService : IPaymentSearchService
    {
        private readonly Func<IOrderRepository> _repositoryFactory;
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly IPaymentService _paymentService;

        public PaymentSearchService(Func<IOrderRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache, IPaymentService paymentService)
        {
            _repositoryFactory = repositoryFactory;
            _platformMemoryCache = platformMemoryCache;
            _paymentService = paymentService;
        }

        public virtual async Task<PaymentSearchResult> SearchPaymentsAsync(PaymentSearchCriteria criteria)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(PaymentSearchCriteria), criteria.GetCacheKey());
            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(OrderSearchCacheRegion.CreateChangeToken());
                using (var repository = _repositoryFactory())
                {
                    repository.DisableChangesTracking();
                    
                    var result = AbstractTypeFactory<PaymentSearchResult>.TryCreateInstance();
      
                    var query = BuildQuery(repository, criteria);
                    var sortInfos = BuildSortExpression(criteria);

                    result.TotalCount = await query.CountAsync();
                    if (criteria.Take > 0)
                    {
                        var ids = await query.OrderBySortInfos(sortInfos).ThenBy(x => x.Id)
                                                         .Select(x => x.Id)
                                                         .Skip(criteria.Skip).Take(criteria.Take)
                                                         .ToArrayAsync();
                        var unorderedResults = await _paymentService.GetByIdsAsync(ids, criteria.ResponseGroup);
                        result.Results = unorderedResults.OrderBy(x => Array.IndexOf(ids, x.Id)).ToList();
                    }                  
                    return result;
                }
            });
        }

        protected virtual IQueryable<PaymentInEntity> BuildQuery(IOrderRepository repository, PaymentSearchCriteria criteria)
        {
            var query = repository.InPayments;

            if (!criteria.Ids.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.Ids.Contains(x.Id));
            }

            if (!string.IsNullOrEmpty(criteria.OrderId))
            {
                query = query.Where(x => x.CustomerOrderId == criteria.OrderId);
            }
            else if (!string.IsNullOrEmpty(criteria.OrderNumber))
            {
                query = query.Where(x => x.CustomerOrder.Number == criteria.OrderNumber);
            }
            if (criteria.EmployeeId != null)
            {
                query = query.Where(x => x.CustomerOrder.EmployeeId == criteria.EmployeeId);
            }
            if (!criteria.StoreIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.StoreIds.Contains(x.CustomerOrder.StoreId));
            }

            if (!criteria.Statuses.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.Statuses.Contains(x.Status));
            }

            if (!criteria.CustomerId.IsNullOrEmpty())
            {
                query = query.Where(x => x.CustomerId == criteria.CustomerId);
            }

            if (criteria.StartDate != null)
            {
                query = query.Where(x => x.CreatedDate >= criteria.StartDate);
            }
            if (criteria.EndDate != null)
            {
                query = query.Where(x => x.CreatedDate <= criteria.EndDate);
            }
            if (criteria.CapturedStartDate != null)
            {
                query = query.Where(x => x.CapturedDate >= criteria.CapturedStartDate);
            }
            if (criteria.CapturedEndDate != null)
            {
                query = query.Where(x => x.CapturedDate <= criteria.CapturedEndDate);
            }
            if (criteria.AuthorizedStartDate != null)
            {
                query = query.Where(x => x.AuthorizedDate >= criteria.AuthorizedStartDate);
            }
            if (criteria.AuthorizedEndDate != null)
            {
                query = query.Where(x => x.AuthorizedDate <= criteria.AuthorizedEndDate);
            }
            if (!criteria.Numbers.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.Numbers.Contains(x.Number));
            }
            else if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(GetKeywordPredicate(criteria));
            }

            return query;
        }

        protected virtual IList<SortInfo> BuildSortExpression(PaymentSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo
                    {
                        SortColumn = nameof(PaymentInEntity.CreatedDate),
                        SortDirection = SortDirection.Descending
                    }
                };
            }
            return sortInfos;
        }

        protected virtual Expression<Func<PaymentInEntity, bool>> GetKeywordPredicate(PaymentSearchCriteria criteria)
        {
            return payment => payment.Number.Contains(criteria.Keyword);
        }
    }
}
