using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.OrdersModule.Data.Search.Indexed
{
    public class CustomerOrderChangesProvider : IIndexDocumentChangesProvider
    {
        public const string ChangeLogObjectType = nameof(CustomerOrder);

        private readonly Func<IOrderRepository> _orderRepositoryFactory;
        private readonly IChangeLogSearchService _changeLogSearchService;

        public CustomerOrderChangesProvider(Func<IOrderRepository> orderRepositoryFactory, IChangeLogSearchService changeLogSearchService)
        {
            _orderRepositoryFactory = orderRepositoryFactory;
            _changeLogSearchService = changeLogSearchService;
        }

        public virtual Task<IList<IndexDocumentChange>> GetChangesAsync(DateTime? startDate, DateTime? endDate, long skip, long take)
        {
            return startDate == null && endDate == null
                ? GetChangesFromRepositoryAsync(skip, take)
                : GetChangesFromOperationLogAsync(startDate, endDate, skip, take);
        }

        public virtual async Task<long> GetTotalChangesCountAsync(DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null && endDate == null)
            {
                // Get total products count
                using var repository = _orderRepositoryFactory();

                return await repository.CustomerOrders.CountAsync();
            }

            var criteria = GetChangeLogSearchCriteria(startDate, endDate, skip: 0, take: 0);

            // Get changes count from operation log
            return (await _changeLogSearchService.SearchAsync(criteria)).TotalCount;
        }

        /// <summary>
        /// Get documents from repository and return them as changes
        /// </summary>
        protected virtual async Task<IList<IndexDocumentChange>> GetChangesFromRepositoryAsync(long skip, long take)
        {
            using var repository = _orderRepositoryFactory();

            var orders = await repository.CustomerOrders
                .OrderBy(x => x.CreatedDate)
                .Select(x => new { x.Id, ModifiedDate = x.ModifiedDate ?? x.CreatedDate })
                .Skip((int)skip)
                .Take((int)take)
                .ToArrayAsync();

            return orders
                .Select(x =>
                    new IndexDocumentChange
                    {
                        DocumentId = x.Id,
                        ChangeType = IndexDocumentChangeType.Modified,
                        ChangeDate = x.ModifiedDate,
                    })
                .ToList();
        }

        /// <summary>
        /// Get changes from operation log
        /// </summary>
        protected virtual async Task<IList<IndexDocumentChange>> GetChangesFromOperationLogAsync(DateTime? startDate, DateTime? endDate, long skip, long take)
        {
            var criteria = GetChangeLogSearchCriteria(startDate, endDate, skip, take);
            var operations = (await _changeLogSearchService.SearchAsync(criteria)).Results;

            return operations
                .Select(x =>
                    new IndexDocumentChange
                    {
                        DocumentId = x.ObjectId,
                        ChangeType = x.OperationType == EntryState.Deleted ? IndexDocumentChangeType.Deleted : IndexDocumentChangeType.Modified,
                        ChangeDate = x.ModifiedDate ?? x.CreatedDate,
                    })
                .ToList();
        }

        protected virtual ChangeLogSearchCriteria GetChangeLogSearchCriteria(DateTime? startDate, DateTime? endDate, long skip, long take)
        {
            var criteria = AbstractTypeFactory<ChangeLogSearchCriteria>.TryCreateInstance();

            var types = AbstractTypeFactory<CustomerOrder>.AllTypeInfos.Select(x => x.TypeName).ToList();

            if (types.Count > 0)
            {
                types.Add(ChangeLogObjectType);
                criteria.ObjectTypes = types;
            }
            else
            {
                criteria.ObjectType = ChangeLogObjectType;
            }

            criteria.StartDate = startDate;
            criteria.EndDate = endDate;
            criteria.Skip = (int)skip;
            criteria.Take = (int)take;

            return criteria;
        }
    }
}
