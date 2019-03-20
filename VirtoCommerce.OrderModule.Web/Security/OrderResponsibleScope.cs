using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.OrderModule.Web.Security
{
    /// <summary>
    /// Scope bounded by order responsible
    /// </summary>
    public class OrderResponsibleScope : PermissionScope
    {
        public OrderResponsibleScope()
        {
            Scope = "{{userId}}";
        }

        public override bool IsScopeAvailableForPermission(string permission)
        {
            return permission == OrderPredefinedPermissions.Read ||
                   permission == OrderPredefinedPermissions.Update;
        }

        public override IEnumerable<string> GetEntityScopeStrings(object entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var customerOrder = entity as CustomerOrder;
            return customerOrder != null
                ? new[] { Type + ":" + customerOrder.EmployeeId }
                : Enumerable.Empty<string>();
        }
    }
}
