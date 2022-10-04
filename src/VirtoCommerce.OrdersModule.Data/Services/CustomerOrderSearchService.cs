using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.OrdersModule.Data.Services
{
    public class CustomerOrderSearchService : SearchService<CustomerOrderSearchCriteria, CustomerOrderSearchResult, CustomerOrder, CustomerOrderEntity>, ICustomerOrderSearchService
    {
        public CustomerOrderSearchService(Func<IOrderRepository> repositoryFactory, ICustomerOrderService customerOrderService, IPlatformMemoryCache platformMemoryCache)
            : base(repositoryFactory, platformMemoryCache, (ICrudService<CustomerOrder>)customerOrderService)
        {
        }

        public virtual Task<CustomerOrderSearchResult> SearchCustomerOrdersAsync(CustomerOrderSearchCriteria criteria)
        {
            return SearchAsync(criteria);
        }

        protected override IQueryable<CustomerOrderEntity> BuildQuery(IRepository repository, CustomerOrderSearchCriteria criteria)
        {
            var query = ((IOrderRepository)repository).CustomerOrders;

            // Don't return prototypes by default
            if (!criteria.WithPrototypes)
            {
                query = query.Where(x => !x.IsPrototype);
            }

            if (!criteria.Ids.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.Ids.Contains(x.Id));
            }

            if (!criteria.Statuses.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.Statuses.Contains(x.Status));
            }

            if (!criteria.StoreIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.StoreIds.Contains(x.StoreId));
            }

            if (!criteria.Numbers.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.Numbers.Contains(x.Number));
            }
            else if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(GetKeywordPredicate(criteria));
            }

            query = WithDateConditions(query, criteria);

            query = WithParentOperationConditions(query, criteria);

            query = WithCustomerConditions(query, criteria);

            query = WithSubscriptionConditions(query, criteria);

            return query;
        }

        protected override IList<SortInfo> BuildSortExpression(CustomerOrderSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo
                    {
                        SortColumn = nameof(CustomerOrderEntity.CreatedDate),
                        SortDirection = SortDirection.Descending
                    }
                };
            }
            return sortInfos;
        }

        protected virtual Expression<Func<CustomerOrderEntity, bool>> GetKeywordPredicate(CustomerOrderSearchCriteria criteria)
        {
            return order => order.Number.Contains(criteria.Keyword) || order.CustomerName.Contains(criteria.Keyword);
        }

        private static IQueryable<CustomerOrderEntity> WithDateConditions(IQueryable<CustomerOrderEntity> query, CustomerOrderSearchCriteria criteria)
        {
            if (criteria.StartDate != null)
            {
                query = query.Where(x => x.CreatedDate >= criteria.StartDate);
            }

            if (criteria.EndDate != null)
            {
                query = query.Where(x => x.CreatedDate <= criteria.EndDate);
            }

            return query;
        }

        private static IQueryable<CustomerOrderEntity> WithParentOperationConditions(IQueryable<CustomerOrderEntity> query, CustomerOrderSearchCriteria criteria)
        {
            if (criteria.HasParentOperation != null)
            {
                query = query.Where(x => criteria.HasParentOperation.Value ? x.ParentOperationId != null : x.ParentOperationId == null);
            }

            if (criteria.ParentOperationId != null)
            {
                query = query.Where(x => x.ParentOperationId == criteria.ParentOperationId);
            }

            return query;
        }

        private static IQueryable<CustomerOrderEntity> WithCustomerConditions(IQueryable<CustomerOrderEntity> query, CustomerOrderSearchCriteria criteria)
        {
            if (!criteria.CustomerIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.CustomerIds.Contains(x.CustomerId));
            }

            if (criteria.EmployeeId != null)
            {
                query = query.Where(x => x.EmployeeId == criteria.EmployeeId);
            }

            return query;
        }

        private static IQueryable<CustomerOrderEntity> WithSubscriptionConditions(IQueryable<CustomerOrderEntity> query, CustomerOrderSearchCriteria criteria)
        {
            if (criteria.OnlyRecurring)
            {
                query = query.Where(x => x.SubscriptionId != null);
            }

            if (!criteria.SubscriptionIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.SubscriptionIds.Contains(x.SubscriptionId));
            }

            return query;
        }
    }
}
