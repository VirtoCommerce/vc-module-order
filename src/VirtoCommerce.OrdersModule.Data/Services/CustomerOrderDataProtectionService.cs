using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.OrdersModule.Core.Search.Indexed;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using static VirtoCommerce.OrdersModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.OrdersModule.Data.Services;

public class CustomerOrderDataProtectionService(
    ICustomerOrderService crudService,
    ICustomerOrderSearchService searchService,
    IIndexedCustomerOrderSearchService indexedSearchService,
    IUserNameResolver userNameResolver,
    SignInManager<ApplicationUser> signInManager)
    : ICustomerOrderDataProtectionService
{
    public async Task<CustomerOrderIndexedSearchResult> SearchCustomerOrdersAsync(CustomerOrderIndexedSearchCriteria criteria)
    {
        var searchResult = await indexedSearchService.SearchCustomerOrdersAsync(criteria);
        await ReduceDetailsForCurrentUser(searchResult.Results, cloned: true);

        return searchResult;
    }

    public virtual async Task<CustomerOrderSearchResult> SearchAsync(CustomerOrderSearchCriteria criteria, bool clone = true)
    {
        var searchResult = await searchService.SearchAsync(criteria, clone);
        await ReduceDetailsForCurrentUser(searchResult.Results, clone);

        return searchResult;
    }

    public virtual async Task<CustomerOrder> GetByNumberAsync(string number, string responseGroup = null, bool clone = true)
    {
        var searchCriteria = AbstractTypeFactory<CustomerOrderSearchCriteria>.TryCreateInstance();
        searchCriteria.Number = number;
        searchCriteria.ResponseGroup = responseGroup;
        searchCriteria.Take = 1;

        var searchResult = await searchService.SearchAsync(searchCriteria, clone);
        await ReduceDetailsForCurrentUser(searchResult.Results, clone);

        return searchResult.Results.FirstOrDefault();
    }

    public virtual async Task<IList<CustomerOrder>> GetAsync(IList<string> ids, string responseGroup = null, bool clone = true)
    {
        var orders = await crudService.GetAsync(ids, responseGroup, clone);
        await ReduceDetailsForCurrentUser(orders, clone);

        return orders;
    }

    public virtual async Task<IList<CustomerOrder>> GetByOuterIdsAsync(IList<string> outerIds, string responseGroup = null, bool clone = true)
    {
        var orders = await crudService.GetByOuterIdsAsync(outerIds, responseGroup, clone);
        await ReduceDetailsForCurrentUser(orders, clone);

        return orders;
    }

    public virtual async Task SaveChangesAsync(IList<CustomerOrder> orders)
    {
        var user = await GetCurrentUser();

        foreach (var order in orders)
        {
            await RestoreDetailsForUser(user, order);
        }

        await crudService.SaveChangesAsync(orders);
    }

    public virtual Task DeleteAsync(IList<string> ids, bool softDelete = false)
    {
        return crudService.DeleteAsync(ids, softDelete);
    }

    protected virtual async Task ReduceDetailsForCurrentUser(IList<CustomerOrder> orders, bool cloned)
    {
        if (orders.IsNullOrEmpty())
        {
            return;
        }

        var user = await GetCurrentUser();

        for (var i = orders.Count - 1; i >= 0; i--)
        {
            orders[i] = await ReduceDetailsForUser(user, orders[i], cloned);
        }
    }

    protected virtual async Task<CustomerOrder> ReduceDetailsForUser(ClaimsPrincipal user, CustomerOrder order, bool cloned)
    {
        if (order is null || await CanReadPrices(user, order))
        {
            return order;
        }

        // If the order has not been cloned yet, clone it to avoid corrupting the cache
        if (!cloned)
        {
            order = order.CloneTyped();
        }

        await RemovePrices(order);

        return order;
    }

    protected virtual async Task RestoreDetailsForUser(ClaimsPrincipal user, CustomerOrder order)
    {
        if (!await CanReadPrices(user, order))
        {
            await RestorePrices(order);
        }
    }


    protected virtual Task<bool> CanReadPrices(ClaimsPrincipal user, CustomerOrder order)
    {
        if (user is null)
        {
            return Task.FromResult(false);
        }

        var result = user.HasGlobalPermission(ReadPrices);

        return Task.FromResult(result);
    }

    protected virtual Task RemovePrices(CustomerOrder order)
    {
        order.ReduceDetails((CustomerOrderResponseGroup.Full & ~CustomerOrderResponseGroup.WithPrices).ToString());

        return Task.CompletedTask;
    }

    protected virtual async Task RestorePrices(CustomerOrder order)
    {
        var originalOrder = await crudService.GetByIdAsync(order.Id);
        if (originalOrder != null)
        {
            order.RestoreDetails(originalOrder);
        }
    }

    protected virtual async Task<ClaimsPrincipal> GetCurrentUser()
    {
        var userName = userNameResolver.GetCurrentUserName();
        if (userName is null)
        {
            return null;
        }

        var user = await signInManager.UserManager.FindByNameAsync(userName);
        if (user is null)
        {
            return null;
        }

        var claimsPrincipal = await signInManager.CreateUserPrincipalAsync(user);

        return claimsPrincipal;
    }
}
