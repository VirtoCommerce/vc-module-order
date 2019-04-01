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

            return Enum.GetValues(typeof(CustomerOrderResponseGroup))
                       .Cast<CustomerOrderResponseGroup>()
                       .Select(x => x.ToString())
                       .Where(x => x.StartsWith("With"))
                       .Select(x => $"{Type}:{x}");
        }


        /// <summary>
        /// The method checks the requested elements and valid for the user
        /// </summary>
        /// <param name="permissions">Array of user permissions</param>
        /// <param name="respGroup">Requested response group</param>
        /// <returns></returns>
        public static string GetAllowedResponseGroups(Permission[] permissions, string respGroup)
        {
            var userResponseGroupItems = permissions
                .Where(x => x.Id.StartsWith(OrderPredefinedPermissions.Read))
                .SelectMany(x => x.AssignedScopes)
                .OfType<OrderLimitResponseScope>()
                .Select(x => x.Scope)
                .ToList();

            //if the user has no restrictions, then return the requested items
            if (!userResponseGroupItems.Any())
            {
                return respGroup;
            }

            //if the user has restrictions, then make an intersection with the requested parameters
            var result = string.Join(",", string.IsNullOrWhiteSpace(respGroup)
                ? userResponseGroupItems
                : respGroup.Split(',').Where(x => userResponseGroupItems.Contains(x)));

            if (string.IsNullOrWhiteSpace(result))
            {
                result = null;
            }

            return result;
        }
    }
}
