using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.OrdersModule.Data.Caching
{
    public class OrderCacheRegion : CancellableCacheRegion<OrderCacheRegion>
    {
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> _entityRegionTokenLookup = new ConcurrentDictionary<string, CancellationTokenSource>();

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
                changeTokens.Add(new CancellationChangeToken(_entityRegionTokenLookup.GetOrAdd(entityId, new CancellationTokenSource()).Token));
            }
            return new CompositeChangeToken(changeTokens);
        }

        public static void ExpireOrder(CustomerOrder order)
        {
            if (_entityRegionTokenLookup.TryRemove(order.Id, out CancellationTokenSource token))
            {
                token.Cancel();
            }
        }
    }
}
