using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.OrderModule.Web.Security
{
    public class OrderLimitResponseScope : PermissionScope
    {
        public override bool IsScopeAvailableForPermission(string permission)
        {
            return permission == OrderPredefinedPermissions.Read;
        }

        public override IEnumerable<string> GetEntityScopeStrings(object entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var customerOrder = entity as CustomerOrder;

            if (customerOrder == null)
            {
                return Enumerable.Empty<string>();
            }

            return Enum.GetValues(typeof(CustomerOrderResponseGroup)).Cast<CustomerOrderResponseGroup>()
                .Select(x => x.ToString()).Where(x => x.StartsWith("With")).Select(x => $"{Type}:{x}");
        }
    }
}
