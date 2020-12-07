using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.Authorization;

namespace VirtoCommerce.OrdersModule.Web.Authorization
{
    public sealed class OrderAuthorizationRequirement : PermissionAuthorizationRequirement
    {
        public OrderAuthorizationRequirement(string permission)
            : base(permission)
        {
        }
    }

    public sealed class OrderAuthorizationHandler : PermissionAuthorizationHandlerBase<OrderAuthorizationRequirement>
    {
        private readonly MvcNewtonsoftJsonOptions _jsonOptions;

        private readonly UserManager<ApplicationUser> _userManager;

        public OrderAuthorizationHandler(IOptions<MvcNewtonsoftJsonOptions> jsonOptions, Func<UserManager<ApplicationUser>> userManager)
        {
            _jsonOptions = jsonOptions.Value;
            _userManager = userManager();
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OrderAuthorizationRequirement requirement)
        {
            await base.HandleRequirementAsync(context, requirement);

            if (!context.HasSucceeded)
            {
                var userPermission = context.User.FindPermission(requirement.Permission, _jsonOptions.SerializerSettings);
                if (userPermission != null)
                {
                    var storeSelectedScopes = userPermission.AssignedScopes.OfType<OrderSelectedStoreScope>();
                    var onlyResponsibleScope = userPermission.AssignedScopes.OfType<OnlyOrderResponsibleScope>().FirstOrDefault();
                    var allowedStoreIds = storeSelectedScopes.Select(x => x.StoreId).Distinct().ToArray();

                    if (context.Resource is OrderOperationSearchCriteriaBase criteria)
                    {
                        criteria.StoreIds = allowedStoreIds;
                        if (onlyResponsibleScope != null)
                        {
                            var identityName = context.User.Identity.Name;
                            var user = await _userManager.GetUserAsync(context.User);
                            criteria.EmployeeId = user != null ? user.MemberId : identityName;
                        }
                        context.Succeed(requirement);
                    }
                    if (context.Resource is CustomerOrder order)
                    {
                        if (allowedStoreIds.Contains(order.StoreId) || (onlyResponsibleScope != null && order.EmployeeId.EqualsInvariant(context.User.Identity.Name)))
                        {
                            context.Succeed(requirement);
                        }
                    }
                }
            }

            //Apply ReadPrices authorization rules for all checks
            if (!context.User.HasGlobalPermission(ModuleConstants.Security.Permissions.ReadPrices))
            {
                if (context.Resource is OrderOperationSearchCriteriaBase criteria)
                {
                    if (string.IsNullOrEmpty(criteria.ResponseGroup))
                    {
                        criteria.ResponseGroup = CustomerOrderResponseGroup.Full.ToString();
                    }
                    criteria.ResponseGroup = EnumUtility.SafeRemoveFlagFromEnumString(criteria.ResponseGroup, CustomerOrderResponseGroup.WithPrices);
                    //Do not allow pass empty response group into services because that can leads to use default response group CustomerOrderResponseGroup.Full
                    if (string.IsNullOrEmpty(criteria.ResponseGroup))
                    {
                        criteria.ResponseGroup = CustomerOrderResponseGroup.Default.ToString();
                    }
                    context.Succeed(requirement);
                }
                if (context.Resource is CustomerOrder order)
                {
                    order.ReduceDetails((CustomerOrderResponseGroup.Full & ~CustomerOrderResponseGroup.WithPrices).ToString());
                }
            }
        }
    }
}
