using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.OrdersModule.Data.Services;

/// <summary>
/// Removes prices from order child operations (payments, shipments) the current user may not read, and
/// restores them before a save. The price rule itself is not reimplemented here: every decision is
/// delegated to <see cref="ICustomerOrderDataProtectionService.CanReadPricesAsync"/> for the operation's
/// parent order, so a custom policy is written once and applies to every entity in the aggregate.
/// </summary>
public abstract class OrderOperationDataProtectionService<TOperation, TCriteria, TResult>(
    ICustomerOrderService orderService,
    ICustomerOrderDataProtectionService orderDataProtectionService,
    IOuterEntityService<TOperation> crudService,
    ISearchService<TCriteria, TResult, TOperation> searchService)
    where TOperation : OrderOperation, IHasOuterId, ICloneable
    where TCriteria : SearchCriteriaBase
    where TResult : GenericSearchResult<TOperation>
{
    /// <summary>
    /// The id of the order the operation belongs to. Payments and shipments name this property differently.
    /// </summary>
    protected abstract string GetOrderId(TOperation operation);

    public virtual async Task<TResult> SearchAsync(TCriteria criteria, bool clone = true)
    {
        var searchResult = await searchService.SearchAsync(criteria, clone);
        await ReduceDetailsForCurrentUser(searchResult.Results, clone);

        return searchResult;
    }

    public virtual async Task<IList<TOperation>> GetAsync(IList<string> ids, string responseGroup = null, bool clone = true)
    {
        var operations = await crudService.GetAsync(ids, responseGroup, clone);
        await ReduceDetailsForCurrentUser(operations, clone);

        return operations;
    }

    public virtual async Task<IList<TOperation>> GetByOuterIdsAsync(IList<string> outerIds, string responseGroup = null, bool clone = true)
    {
        var operations = await crudService.GetByOuterIdsAsync(outerIds, responseGroup, clone);
        await ReduceDetailsForCurrentUser(operations, clone);

        return operations;
    }

    public virtual async Task SaveChangesAsync(IList<TOperation> operations)
    {
        await RestoreDetailsForCurrentUser(operations);

        await crudService.SaveChangesAsync(operations);
    }

    public virtual Task DeleteAsync(IList<string> ids, bool softDelete = false)
    {
        return crudService.DeleteAsync(ids, softDelete);
    }


    protected virtual async Task ReduceDetailsForCurrentUser(IList<TOperation> operations, bool cloned)
    {
        if (operations.IsNullOrEmpty())
        {
            return;
        }

        var allowedOrderIds = await GetOrderIdsWithReadablePrices(operations);

        for (var i = operations.Count - 1; i >= 0; i--)
        {
            var operation = operations[i];
            if (operation is null || allowedOrderIds.Contains(GetOrderId(operation)))
            {
                continue;
            }

            // If the operation has not been cloned yet, clone it to avoid corrupting the cache
            if (!cloned)
            {
                operation = (TOperation)operation.Clone();
                operations[i] = operation;
            }

            RemovePrices(operation);
        }
    }

    protected virtual async Task RestoreDetailsForCurrentUser(IList<TOperation> operations)
    {
        if (operations.IsNullOrEmpty())
        {
            return;
        }

        var allowedOrderIds = await GetOrderIdsWithReadablePrices(operations);

        var restricted = operations
            .Where(x => x != null && !x.Id.IsNullOrEmpty() &&
                        (!x.WithPrices || !allowedOrderIds.Contains(GetOrderId(x))))
            .ToList();

        if (restricted.Count == 0)
        {
            return;
        }

        await RestorePrices(restricted);
    }

    protected virtual async Task RestorePrices(IList<TOperation> operations)
    {
        var originalOperations = await crudService.GetAsync(operations.Select(x => x.Id).Distinct().ToList());

        var originalById = originalOperations
            .Where(x => x != null)
            .GroupBy(x => x.Id)
            .ToDictionary(x => x.Key, x => x.First());

        foreach (var operation in operations)
        {
            // A new operation has no stored counterpart and therefore nothing to restore
            if (originalById.TryGetValue(operation.Id, out var original))
            {
                operation.RestoreDetails(original);
            }
        }
    }

    protected virtual void RemovePrices(TOperation operation)
    {
        operation.ReduceDetails((CustomerOrderResponseGroup.Full & ~CustomerOrderResponseGroup.WithPrices).ToString());
    }

    /// <summary>
    /// Resolves the parent orders of the given operations once and returns the ids of those
    /// whose prices the current user may read.
    /// </summary>
    protected virtual async Task<ISet<string>> GetOrderIdsWithReadablePrices(IList<TOperation> operations)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var orderIds = operations
            .Where(x => x != null)
            .Select(GetOrderId)
            .Where(x => !x.IsNullOrEmpty())
            .Distinct()
            .ToList();

        if (orderIds.Count == 0)
        {
            return result;
        }

        // Read-only: the orders are used to make the decision and are never modified, so no clone is needed
        var orders = await orderService.GetAsync(orderIds, CustomerOrderResponseGroup.Full.ToString(), clone: false);

        foreach (var order in orders)
        {
            if (await orderDataProtectionService.CanReadPricesAsync(order))
            {
                result.Add(order.Id);
            }
        }

        return result;
    }
}
