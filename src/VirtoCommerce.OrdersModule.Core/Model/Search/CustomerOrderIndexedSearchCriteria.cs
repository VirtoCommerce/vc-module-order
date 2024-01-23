using System.Collections.Generic;

namespace VirtoCommerce.OrdersModule.Core.Model.Search
{
    public class CustomerOrderIndexedSearchCriteria : CustomerOrderSearchCriteria
    {
        public string Facet { get; set; }
    }

    public class CustomerOrderIndexedSearchResult : CustomerOrderSearchResult
    {
        public virtual IList<OrderAggregation> Aggregations { get; set; }
    }

    public class OrderAggregation
    {
        /// <summary>
        /// Gets or sets the value of the aggregation type
        /// </summary>
        /// <value>
        /// "Attribute", "Range"
        /// </value>
        public string AggregationType { get; set; }

        /// <summary>
        /// Gets or sets the value of the aggregation field
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Gets or sets the collection of the aggregation labels
        /// </summary>
        public IList<OrderAggregationLabel> Labels { get; set; }

        /// <summary>
        /// Gets or sets the collection of the aggregation items
        /// </summary>
        public IList<OrderAggregationItem> Items { get; set; }
    }

    public class OrderAggregationLabel
    {
        public string Language { get; set; }
        public string Label { get; set; }
    }

    public class OrderAggregationItem
    {
        /// <summary>
        /// Gets or sets the aggregation item value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the aggregation item count
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the flag for aggregation item is applied
        /// </summary>
        public bool IsApplied { get; set; }

        /// <summary>
        /// Gets or sets the collection of the aggregation item labels
        /// </summary>
        public IList<OrderAggregationLabel> Labels { get; set; }
    }
}
