using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.OrdersModule2.Web.Authorization.Extensions
{
    public static class PermissionExtensions
    {
        public static IEnumerable<string> GetAllowedStatuses<T>(this Permission permission) where T : PermissionScope
        {
            return permission.AssignedScopes.OfType<T>().Select(x => x.Scope).Distinct();
        }
    }
}
