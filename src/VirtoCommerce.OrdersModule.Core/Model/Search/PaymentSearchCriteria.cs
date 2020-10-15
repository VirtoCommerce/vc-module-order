using System;

namespace VirtoCommerce.OrdersModule.Core.Model.Search
{
    public class PaymentSearchCriteria : OrderOperationSearchCriteriaBase
    {
        /// <summary>
        /// It used to limit search within a customer order id
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// It used to limit search within a customer order number
        /// </summary>
        public string OrderNumber { get; set; }

        /// <summary>
        /// Filter payments by customer 
        /// </summary>
        public string CustomerId { get; set; }

        public DateTime? CapturedStartDate { get; set; }
        public DateTime? CapturedEndDate { get; set; }

        public DateTime? AuthorizedStartDate { get; set; }
        public DateTime? AuthorizedEndDate { get; set; }
    }
}
