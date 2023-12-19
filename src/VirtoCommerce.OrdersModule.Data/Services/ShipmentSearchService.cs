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
    public class ShipmentSearchService : SearchService<ShipmentSearchCriteria, ShipmentSearchResult, Shipment, ShipmentEntity>, IShipmentSearchService
    {
        public ShipmentSearchService(
            Func<IOrderRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IShipmentService crudService,
            IOptions<CrudOptions> crudOptions)
            : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
        {
        }


        protected override IQueryable<ShipmentEntity> BuildQuery(IRepository repository, ShipmentSearchCriteria criteria)
        {
            var query = ((IOrderRepository)repository).Shipments;

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

            if (criteria.StartDate != null)
            {
                query = query.Where(x => x.CreatedDate >= criteria.StartDate);
            }

            if (criteria.EndDate != null)
            {
                query = query.Where(x => x.CreatedDate <= criteria.EndDate);
            }

            if (!criteria.Numbers.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.Numbers.Contains(x.Number));
            }
            else if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(GetKeywordPredicate(criteria));
            }

            if (!string.IsNullOrEmpty(criteria.FulfillmentCenterId))
            {
                query = query.Where(x => x.FulfillmentCenterId == criteria.FulfillmentCenterId);
            }

            if (!string.IsNullOrEmpty(criteria.ShipmentMethodCode))
            {
                query = query.Where(x => x.ShipmentMethodCode == criteria.ShipmentMethodCode);
            }

            if (!string.IsNullOrEmpty(criteria.ShipmentMethodOption))
            {
                query = query.Where(x => x.ShipmentMethodOption == criteria.ShipmentMethodOption);
            }

            return query;
        }

        protected override IList<SortInfo> BuildSortExpression(ShipmentSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo
                    {
                        SortColumn = nameof(ShipmentEntity.CreatedDate),
                        SortDirection = SortDirection.Descending
                    }
                };
            }
            return sortInfos;
        }

        protected virtual Expression<Func<ShipmentEntity, bool>> GetKeywordPredicate(ShipmentSearchCriteria criteria)
        {
            return Shipment => Shipment.Number.Contains(criteria.Keyword);
        }
    }
}
