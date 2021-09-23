using System;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model.Search;

namespace VirtoCommerce.OrdersModule.Core.Services
{
    /// <summary>
    /// This interface should implement <![CDATA[<see cref="SearchService<CustomerOrder>"/>]]> without methods.
    /// Methods left for compatibility and should be removed after upgrade to inheritance
    /// </summary>
    public interface ICustomerOrderSearchService
    {
        [Obsolete(@"Need to remove after inherit ICustomerOrderSearchService from SearchService<CustomerOrder>")]
        Task<CustomerOrderSearchResult> SearchCustomerOrdersAsync(CustomerOrderSearchCriteria criteria);
    }
}
