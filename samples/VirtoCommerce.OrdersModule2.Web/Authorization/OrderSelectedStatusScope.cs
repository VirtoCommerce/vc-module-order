using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.OrdersModule2.Web.Authorization
{
    public sealed class OrderSelectedStatusScope : PermissionScope
    {
        public string Status => Scope;
    }
}
