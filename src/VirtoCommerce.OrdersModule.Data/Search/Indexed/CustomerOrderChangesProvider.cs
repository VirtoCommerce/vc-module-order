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
            IList<IndexDocumentChange> result;

            if (startDate == null && endDate == null)
            {
                // Get documents from repository and return them as changes
                using (var repository = _orderRepositoryFactory())
                {
                    var productIds = repository.CustomerOrders
                        .OrderBy(i => i.CreatedDate)
                        .Select(i => i.Id)
                        .Skip((int)skip)
                        .Take((int)take)
                        .ToArray();

                    result = productIds.Select(id =>
                        new IndexDocumentChange
                        {
                            DocumentId = id,
                            ChangeType = IndexDocumentChangeType.Modified,
                            ChangeDate = DateTime.UtcNow
                        }
                    ).ToArray();
                }
            }
            else
            {
                var criteria = new ChangeLogSearchCriteria
                {
                    ObjectType = ChangeLogObjectType,
                    StartDate = startDate,
                    EndDate = endDate,
                    Skip = (int)skip,
                    Take = (int)take
                };

                // Get changes from operation log
                var operations = (await _changeLogSearchService.SearchAsync(criteria)).Results;

                result = operations.Select(o =>
                    new IndexDocumentChange
                    {
                        DocumentId = o.ObjectId,
                        ChangeType = o.OperationType == EntryState.Deleted ? IndexDocumentChangeType.Deleted : IndexDocumentChangeType.Modified,
                        ChangeDate = o.ModifiedDate ?? o.CreatedDate,
                    }
                ).ToArray();
            }

            return result;
        }

        public async Task<long> GetTotalChangesCountAsync(DateTime? startDate, DateTime? endDate)
        {
            long result;

            if (startDate == null && endDate == null)
            {
                // Get total products count
                using (var repository = _orderRepositoryFactory())
                {
                    result = repository.CustomerOrders.Count();
                }
            }
            else
            {
                var criteria = new ChangeLogSearchCriteria
                {
                    ObjectType = ChangeLogObjectType,
                    StartDate = startDate,
                    EndDate = endDate,
                    Take = 0
                };
                // Get changes count from operation log
                result = (await _changeLogSearchService.SearchAsync(criteria)).TotalCount;
            }

            return result;
        }
    }
}
