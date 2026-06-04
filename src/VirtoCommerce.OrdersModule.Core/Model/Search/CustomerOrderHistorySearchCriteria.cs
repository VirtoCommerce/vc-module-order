using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Core.Model.Search
{
    public class CustomerOrderHistorySearchCriteria : SearchCriteriaBase
    {
        public string OrderId { get; set; }
        public string[] OperationTypes { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
