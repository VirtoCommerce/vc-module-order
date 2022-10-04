using VirtoCommerce.Platform.Security.Authorization;

namespace VirtoCommerce.OrdersModule.Data.Authorization
{
    public sealed class OrderAuthorizationRequirement : PermissionAuthorizationRequirement
    {
        public OrderAuthorizationRequirement(string permission)
            : base(permission)
        {
        }
    }
}
