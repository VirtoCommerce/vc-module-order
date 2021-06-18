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

        private readonly Func<IOrderRepository> _orderRepositoryFactory;
        private readonly IChangeLogSearchService _changeLogSearchService;

        public CustomerOrderChangesProvider(Func<IOrderRepository> orderRepositoryFactory, IChangeLogSearchService changeLogSearchService)
        {
            _orderRepositoryFactory = orderRepositoryFactory;
            _changeLogSearchService = changeLogSearchService;
        }

        public async Task<IList<IndexDocumentChange>> GetChangesAsync(DateTime? startDate, DateTime? endDate, long skip, long take)
        {
            if (startDate == null && endDate == null)
            {
                return GetChangesFromRepository(skip, take);
            }

            return await GetChangesFromOperaionLog(startDate, endDate, skip, take);
        }

        public async Task<long> GetTotalChangesCountAsync(DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null && endDate == null)
            {
                // Get total products count
                using (var repository = _orderRepositoryFactory())
                {
                    return repository.CustomerOrders.Count();
                }
            }

            var criteria = new ChangeLogSearchCriteria
            {
                ObjectType = ChangeLogObjectType,
                StartDate = startDate,
                EndDate = endDate,
                Take = 0
            };

            // Get changes count from operation log
            return (await _changeLogSearchService.SearchAsync(criteria)).TotalCount;
        }

        /// <summary>
        /// Get documents from repository and return them as changes
        /// </summary>
        private IList<IndexDocumentChange> GetChangesFromRepository(long skip, long take)
        {
            using (var repository = _orderRepositoryFactory())
            {
                var productIds = repository.CustomerOrders
                    .OrderBy(i => i.CreatedDate)
                    .Select(i => i.Id)
                    .Skip((int)skip)
                    .Take((int)take)
                    .ToArray();

                return productIds.Select(id =>
                    new IndexDocumentChange
                    {
                        DocumentId = id,
                        ChangeType = IndexDocumentChangeType.Modified,
                        ChangeDate = DateTime.UtcNow
                    }
                ).ToArray();
            }
        }

        /// <summary>
        /// Get changes from operation log
        /// </summary>
        private async Task<IList<IndexDocumentChange>> GetChangesFromOperaionLog(DateTime? startDate, DateTime? endDate, long skip, long take)
        {
            var criteria = new ChangeLogSearchCriteria
            {
                ObjectType = ChangeLogObjectType,
                StartDate = startDate,
                EndDate = endDate,
                Skip = (int)skip,
                Take = (int)take
            };

            var operations = (await _changeLogSearchService.SearchAsync(criteria)).Results;

            return operations.Select(o =>
                new IndexDocumentChange
                {
                    DocumentId = o.ObjectId,
                    ChangeType = o.OperationType == EntryState.Deleted ? IndexDocumentChangeType.Deleted : IndexDocumentChangeType.Modified,
                    ChangeDate = o.ModifiedDate ?? o.CreatedDate,
                }
            ).ToArray();
        }
    }
}
