using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.OrderModule.Web.Security
{
    public class OrderLimitResponseScope : PermissionScope
    {
        public override bool IsScopeAvailableForPermission(string permission)
        {
            return permission == OrderPredefinedPermissions.Read;
        }
    }
}
