namespace VirtoCommerce.OrdersModule.Core.Model.Search
{
    public class ShipmentSearchCriteria : OrderOperationSearchCriteriaBase
    {
        /// <summary>
        /// It used to limit search within a customer order id
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// It used to limit search within a customer order number
        /// </summary>
        public string OrderNumber { get; set; }

        public string FulfillmentCenterId { get; set; }

        public string ShipmentMethodCode { get; set; }

        public string ShipmentMethodOption { get; set; }
    }
}
