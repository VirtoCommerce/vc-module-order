using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model.Search;

namespace VirtoCommerce.OrdersModule.Core.Search.Indexed
{
    public interface IIndexedCustomerOrderSearchService
    {
        Task<CustomerOrderIndexedSearchResult> SearchCustomerOrdersAsync(CustomerOrderIndexedSearchCriteria criteria);
    }
}
