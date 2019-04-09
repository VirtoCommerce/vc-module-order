using System.Linq;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.OrderModule.Web.Security
{
    public static class ReadPricesPermission
    {
        public static string Check(Permission[] permissions, string respGroup)
        {
            var result = respGroup;

            var needRestrict = permissions.Any() && permissions.All(x => x.Id != OrderPredefinedPermissions.ReadPrices);

            if (needRestrict && string.IsNullOrWhiteSpace(respGroup))
            {
                const CustomerOrderResponseGroup val =
                    CustomerOrderResponseGroup.Full & ~CustomerOrderResponseGroup.WithPrices;

                result = val.ToString();
            }
            else if (needRestrict)
            {
                var items = respGroup.Split(',').Select(x => x.Trim()).ToList();

                items.Remove(CustomerOrderResponseGroup.WithPrices.ToString());

                result = string.Join(",", items);
            }

            return result;
        }
    }
}
