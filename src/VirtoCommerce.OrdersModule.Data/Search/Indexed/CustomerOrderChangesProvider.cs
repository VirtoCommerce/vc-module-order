using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        protected Func<IOrderRepository> OrderRepositoryFactory { get; }

        protected IChangeLogSearchService ChangeLogSearchService { get; }

        public CustomerOrderChangesProvider(Func<IOrderRepository> orderRepositoryFactory, IChangeLogSearchService changeLogSearchService)
        {
            OrderRepositoryFactory = orderRepositoryFactory;
            ChangeLogSearchService = changeLogSearchService;
        }

        public virtual async Task<IList<IndexDocumentChange>> GetChangesAsync(DateTime? startDate, DateTime? endDate, long skip, long take)
        {
            if (startDate == null && endDate == null)
            {
                return GetChangesFromRepository(skip, take);
            }

            return await GetChangesFromOperationLog(startDate, endDate, skip, take);
        }

        public virtual async Task<long> GetTotalChangesCountAsync(DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null && endDate == null)
            {
                // Get total products count
                using var repository = OrderRepositoryFactory();

                return repository.CustomerOrders.Count();
            }

            var criteria = GetChangeLogSearchCriteria(startDate, endDate, 0, 0);

            // Get changes count from operation log
            return (await ChangeLogSearchService.SearchAsync(criteria)).TotalCount;
        }

        /// <summary>
        /// Get documents from repository and return them as changes
        /// </summary>
        protected virtual IList<IndexDocumentChange> GetChangesFromRepository(long skip, long take)
        {
            using var repository = OrderRepositoryFactory();

            var productIds = repository.CustomerOrders
                .OrderBy(x => x.CreatedDate)
                .Select(x => new { x.Id, ModifiedDate = x.ModifiedDate ?? x.CreatedDate })
                .Skip((int)skip)
                .Take((int)take)
                .ToArray();

            return productIds
                .Select(x =>
                    new IndexDocumentChange
                    {
                        DocumentId = x.Id,
                        ChangeType = IndexDocumentChangeType.Modified,
                        ChangeDate = x.ModifiedDate,
                    })
                .ToArray();
        }

        /// <summary>
        /// Get changes from operation log
        /// </summary>
        protected virtual async Task<IList<IndexDocumentChange>> GetChangesFromOperationLog(DateTime? startDate, DateTime? endDate, long skip, long take)
        {
            var criteria = GetChangeLogSearchCriteria(startDate, endDate, skip, take);
            var operations = (await ChangeLogSearchService.SearchAsync(criteria)).Results;

            return operations
                .Select(x =>
                    new IndexDocumentChange
                    {
                        DocumentId = x.ObjectId,
                        ChangeType = x.OperationType == EntryState.Deleted ? IndexDocumentChangeType.Deleted : IndexDocumentChangeType.Modified,
                        ChangeDate = x.ModifiedDate ?? x.CreatedDate,
                    })
                .ToArray();
        }

        protected virtual ChangeLogSearchCriteria GetChangeLogSearchCriteria(DateTime? startDate, DateTime? endDate, long skip, long take)
        {
            var criteria = AbstractTypeFactory<ChangeLogSearchCriteria>.TryCreateInstance();

            var types = AbstractTypeFactory<CustomerOrder>.AllTypeInfos.Select(x => x.TypeName).ToList();

            if (types.Count != 0)
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
