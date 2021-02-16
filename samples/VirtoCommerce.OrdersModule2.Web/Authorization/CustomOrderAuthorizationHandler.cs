using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.OrdersModule.Data.Authorization;
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

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OrderAuthorizationRequirement requirement)
        {
            //complementary permission checking for the main authentication handler OrderAuthorizationHandler
            //only for if previous handler returns success
            if (context.HasSucceeded)
            {
                var userPermission = context.User.FindPermission(requirement.Permission, _jsonOptions.SerializerSettings);
                if (userPermission != null)
                {
                    var selectedStatusesScopes = userPermission.AssignedScopes.OfType<OrderSelectedStatusScope>();
                    var allowedStatuses = selectedStatusesScopes.Select(x => x.Status).Distinct().ToArray();

                    if (context.Resource is OrderOperationSearchCriteriaBase criteria)
                    {
                        criteria.Statuses = allowedStatuses;
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
}

