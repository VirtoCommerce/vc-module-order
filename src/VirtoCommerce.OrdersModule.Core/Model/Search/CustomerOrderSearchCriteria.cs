using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Core.Model.Search
{
    public class CustomerOrderSearchCriteria : OrderOperationSearchCriteriaBase
    {
        /// <summary>
        /// Search orders with flag IsPrototype
        /// </summary>
        public bool WithPrototypes { get; set; }

        /// <summary>
        /// Search only recurring orders created by subscription
        /// </summary>
        public bool OnlyRecurring { get; set; }

        /// <summary>
        /// Search orders with given subscription
        /// </summary>
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Search orders with given subscriptions
        /// </summary>
        private string[] _subscriptionIds;
        public string[] SubscriptionIds
        {
            get
            {
                if (_subscriptionIds == null && !string.IsNullOrEmpty(SubscriptionId))
                {
                    _subscriptionIds = new[] { SubscriptionId };
                }
                return _subscriptionIds;
            }
            set
            {
                _subscriptionIds = value;
            }
        }



        /// <summary>
        /// It used to limit search within an operation (customer order for example)
        /// </summary>
        public string OperationId { get; set; }


        public string CustomerId { get; set; }

        private string[] _customerIds;
        public string[] CustomerIds
        {
            get
            {
                if (_customerIds.IsNullOrEmpty() && !string.IsNullOrEmpty(CustomerId))
                {
                    _customerIds = new[] { CustomerId };
                }
                return _customerIds;
            }
            set
            {
                _customerIds = value;
            }
        }

        public string OrganizationId { get; set; }

        private string[] _organizationIds;
        public string[] OrganizationIds
        {
            get
            {
                if (_organizationIds.IsNullOrEmpty() && !string.IsNullOrEmpty(OrganizationId))
                {
                    _organizationIds = new[] { OrganizationId };
                }
                return _organizationIds;
            }
            set
            {
                _organizationIds = value;
            }
        }

        /// <summary>
        /// Search orders with a certain product
        /// </summary>
        public string ProductId { get; set; }
    }
}
