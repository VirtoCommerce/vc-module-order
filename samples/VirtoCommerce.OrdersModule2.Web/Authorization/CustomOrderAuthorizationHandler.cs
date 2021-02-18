using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.OrdersModule.Data.Authorization;
using VirtoCommerce.OrdersModule2.Web.Authorization.Extensions;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.Authorization;

namespace VirtoCommerce.OrdersModule2.Web.Authorization
{
    public sealed class CustomOrderAuthorizationHandler : PermissionAuthorizationHandlerBase<OrderAuthorizationRequirement>
    {
        private readonly MvcNewtonsoftJsonOptions _jsonOptions;

        public CustomOrderAuthorizationHandler(IOptions<MvcNewtonsoftJsonOptions> jsonOptions)
        {
            _jsonOptions = jsonOptions.Value;
        }

        /// <summary>
        /// Сomplementary permission checking for the main authentication handler OrderAuthorizationHandler
        /// </summary>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OrderAuthorizationRequirement requirement)
        {
            // Go next only if previous handler returns success
            if (!context.HasSucceeded)
                return;

            var userPermission = context.User.FindPermission(requirement.Permission, _jsonOptions.SerializerSettings);
            if (userPermission != null)
            {
                var allowedStatuses = userPermission.GetAllowedStatuses<OrderSelectedStatusScope>();

                // TODO: C# 9.0 => Rewrite to switch with 'and pattern' when C# 9.0 is available
                if (context.Resource is OrderOperationSearchCriteriaBase criteria)
                {
                    criteria.Statuses = criteria.Statuses.Intersect(allowedStatuses).ToArray();
                    context.Succeed(requirement);
                }

                if (context.Resource is CustomerOrder order && !allowedStatuses.Contains(order.Status))
                {
                    context.Fail();
                }
            }
        }
    }
}
