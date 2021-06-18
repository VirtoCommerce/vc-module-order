using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Core.Search.Indexed
{
    public interface IIndexedCustomerOrderSearchService
    {
        Task<GenericSearchResult<CustomerOrder>> SearchCustomerOrdersAsync(CustomerOrderIndexedSearchCriteria criteria);
    }
}
