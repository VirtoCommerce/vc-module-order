using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Services;

public interface ICustomerOrderDataProtectionService : ICustomerOrderSearchService, ICustomerOrderService
{
    Task<CustomerOrder> GetByNumberAsync(string number, string responseGroup = null, bool clone = true);

    Task ReduceDetailsForCurrentUser(IList<CustomerOrder> orders, bool cloned);
}
