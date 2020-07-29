using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.OrdersModule.Data.Caching
{
    public class OrderCacheRegion : CancellableCacheRegion<OrderCacheRegion>
    {
        public static IChangeToken CreateChangeToken(CustomerOrder[] orders)
        {
            if (orders == null)
            {
                throw new ArgumentNullException(nameof(orders));
            }
            return CreateChangeToken(orders.Select(x => x.Id).ToArray());
        }

        public static IChangeToken CreateChangeToken(string[] entityIds)
        {
            if (entityIds == null)
            {
                throw new ArgumentNullException(nameof(entityIds));
            }

            var changeTokens = new List<IChangeToken> { CreateChangeToken() };
            foreach (var entityId in entityIds)
            {
                changeTokens.Add(CreateChangeTokenForKey(entityId));
            }
            return new CompositeChangeToken(changeTokens);
        }

        public static void ExpireOrder(CustomerOrder order)
        {
            ExpireTokenForKey(order.Id);
        }
    }
}
