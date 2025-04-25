using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.OrdersModule.Data.Search.Indexed;
public class PurchasedProductsChangesProvider : IIndexDocumentChangesProvider
{
    public const string ChangeLogObjectType = nameof(CustomerOrder);

    private readonly Func<IOrderRepository> _orderRepositoryFactory;
    private readonly ICustomerOrderService _customerOrderService;
    private readonly IChangeLogSearchService _changeLogSearchService;

    public PurchasedProductsChangesProvider(
        Func<IOrderRepository> orderRepositoryFactory,
        ICustomerOrderService customerOrderService,
        IChangeLogSearchService changeLogSearchService)
    {
        _orderRepositoryFactory = orderRepositoryFactory;
        _customerOrderService = customerOrderService;
        _changeLogSearchService = changeLogSearchService;
    }

    public async Task<IList<IndexDocumentChange>> GetChangesAsync(DateTime? startDate, DateTime? endDate, long skip, long take)
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
        var orders = await _customerOrderService.GetAsync(operations.Select(x => x.ObjectId).Distinct().ToArray(), CustomerOrderResponseGroup.WithItems.ToString());
        var productIds = orders.SelectMany(x => x.Items).Select(x => x.ProductId).Distinct().ToArray();

        var results = productIds.Select(x => new IndexDocumentChange
        {
            DocumentId = x,
            ChangeType = IndexDocumentChangeType.Modified, // dunno how to correctly get Deleted
        }).ToList();

        return results;
    }

    public async Task<long> GetTotalChangesCountAsync(DateTime? startDate, DateTime? endDate)
    {
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
}
