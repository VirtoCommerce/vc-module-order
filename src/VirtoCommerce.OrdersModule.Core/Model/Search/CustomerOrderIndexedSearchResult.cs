using System.Collections.Generic;

namespace VirtoCommerce.OrdersModule.Core.Model.Search
{
    public class CustomerOrderIndexedSearchResult : CustomerOrderSearchResult
    {
        public virtual IList<OrderAggregation> Aggregations { get; set; }
    }
}
