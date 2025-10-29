using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Search.Indexed;

namespace VirtoCommerce.OrdersModule.Core.Services;

public interface ICustomerOrderDataProtectionService : IIndexedCustomerOrderSearchService, ICustomerOrderSearchService, ICustomerOrderService
{
    Task<CustomerOrder> GetByNumberAsync(string number, string responseGroup = null, bool clone = true);
}
