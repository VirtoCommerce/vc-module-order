using System;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.OrdersModule.Data.Caching
{
    public class OrderCacheRegion : CancellableCacheRegion<OrderCacheRegion>
    {
        public static IChangeToken CreateChangeToken(CustomerOrder order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }
            return CreateChangeTokenForKey(order.Id);
        }

        public static void ExpireOrder(CustomerOrder order)
        {
            ExpireTokenForKey(order.Id);
        }
    }
}
