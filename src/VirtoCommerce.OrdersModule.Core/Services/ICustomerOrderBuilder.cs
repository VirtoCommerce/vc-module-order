using System.Threading.Tasks;
using VirtoCommerce.CartModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Services
{
    public interface ICustomerOrderBuilder
    {
        Task<CustomerOrder> PlaceCustomerOrderFromCartAsync(ShoppingCart cart);
    }
}
