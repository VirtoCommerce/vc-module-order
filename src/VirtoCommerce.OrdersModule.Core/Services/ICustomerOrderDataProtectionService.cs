using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Search.Indexed;

namespace VirtoCommerce.OrdersModule.Core.Services;

public interface ICustomerOrderDataProtectionService : IIndexedCustomerOrderSearchService, ICustomerOrderSearchService, ICustomerOrderService
{
    Task<CustomerOrder> GetByNumberAsync(string number, string responseGroup = null, bool clone = true);

    /// <summary>
    /// Tells whether the current user may read the prices of the given order.
    /// This is the single place the rule is evaluated: the payment and shipment data protection
    /// services defer to it, so a custom policy is implemented once rather than once per entity.
    /// </summary>
    Task<bool> CanReadPricesAsync(CustomerOrder order);
}
