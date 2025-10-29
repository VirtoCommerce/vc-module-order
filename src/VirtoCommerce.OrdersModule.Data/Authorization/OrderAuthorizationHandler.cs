using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.Authorization;
using static VirtoCommerce.Platform.Core.PlatformConstants.Security.Claims;

namespace VirtoCommerce.OrdersModule.Data.Authorization;

public class OrderAuthorizationHandler(IOptions<MvcNewtonsoftJsonOptions> jsonOptions)
    : PermissionAuthorizationHandlerBase<OrderAuthorizationRequirement>
{
    private readonly MvcNewtonsoftJsonOptions _jsonOptions = jsonOptions.Value;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OrderAuthorizationRequirement requirement)
    {
        await base.HandleRequirementAsync(context, requirement);

        var orderContext = GetOrderAuthorizationContext(context, requirement);

        if (!context.HasSucceeded && orderContext.UserPermission != null)
        {
            var succeed = false;

            if (context.Resource is CustomerOrder order)
            {
                succeed = HandleRequirement(order, orderContext);
            }
            else if (context.Resource is OrderOperationSearchCriteriaBase criteria)
            {
                succeed = HandleRequirement(criteria, orderContext);
            }

            if (succeed)
            {
                context.Succeed(requirement);
            }
        }
    }

    protected virtual OrderAuthorizationContext GetOrderAuthorizationContext(AuthorizationHandlerContext context, OrderAuthorizationRequirement requirement)
    {
        var orderContext = AbstractTypeFactory<OrderAuthorizationContext>.TryCreateInstance();

        orderContext.HandlerContext = context;
        orderContext.Requirement = requirement;

        orderContext.UserId = context.User.GetUserId().EmptyToNull();
        orderContext.MemberId = context.User.FindFirstValue(MemberIdClaimType).EmptyToNull();
        orderContext.EmployeeId = orderContext.MemberId ?? orderContext.UserId;

        orderContext.UserPermission = context.User.FindPermission(requirement.Permission, _jsonOptions.SerializerSettings);

        if (orderContext.UserPermission != null)
        {
            orderContext.AllowedStoreIds = orderContext.UserPermission.AssignedScopes.OfType<OrderSelectedStoreScope>().Select(x => x.StoreId).DistinctIgnoreCase().ToArray();
            orderContext.HasResponsibleScope = orderContext.UserPermission.AssignedScopes.OfType<OnlyOrderResponsibleScope>().Any();
        }

        return orderContext;
    }

    protected virtual bool HandleRequirement(CustomerOrder order, OrderAuthorizationContext context)
    {
        return context.AllowedStoreIds.ContainsIgnoreCase(order.StoreId) ||
               (context.HasResponsibleScope && order.EmployeeId.EqualsIgnoreCase(context.EmployeeId));
    }

    protected virtual bool HandleRequirement(OrderOperationSearchCriteriaBase criteria, OrderAuthorizationContext context)
    {
        if (!context.AllowedStoreIds.IsNullOrEmpty())
        {
            criteria.StoreIds = criteria.StoreIds.IsNullOrEmpty()
                ? context.AllowedStoreIds
                : context.AllowedStoreIds.Intersect(criteria.StoreIds ?? []).ToArray();
        }

        if (context.HasResponsibleScope)
        {
            criteria.EmployeeId = context.EmployeeId;
        }

        return true;
    }
}
