using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Core.Model.Search
{
    public class CustomerOrderSearchResult : GenericSearchResult<CustomerOrder>
    {
        //TODO: Urgent! Need to remove it before 3.0 release GA
        [Obsolete("This property is left for backward compatibility reasons and it leads to duplicate response size due to duplicated Result property.")]
        public IList<CustomerOrder> CustomerOrders => Results;

    }
}
