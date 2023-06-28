using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Options;
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
    public class PaymentSearchService : SearchService<PaymentSearchCriteria, PaymentSearchResult, PaymentIn, PaymentInEntity>, IPaymentSearchService
    {
        public PaymentSearchService(
            Func<IOrderRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IPaymentService crudService,
            IOptions<CrudOptions> crudOptions)
            : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
        {
        }


        protected override IQueryable<PaymentInEntity> BuildQuery(IRepository repository, PaymentSearchCriteria criteria)
        {
            var query = ((IOrderRepository)repository).InPayments;

            if (!criteria.Ids.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.Ids.Contains(x.Id));
            }

            if (criteria.HasParentOperation != null)
            {
                query = query.Where(x => criteria.HasParentOperation.Value ? x.ParentOperationId != null : x.ParentOperationId == null);
            }

            if (criteria.ParentOperationId != null)
            {
                query = query.Where(x => x.ParentOperationId == criteria.ParentOperationId);
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

        protected override IList<SortInfo> BuildSortExpression(PaymentSearchCriteria criteria)
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
