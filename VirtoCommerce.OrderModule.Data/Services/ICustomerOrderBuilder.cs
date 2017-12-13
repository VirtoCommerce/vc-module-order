using VirtoCommerce.Domain.Cart.Model;
using VirtoCommerce.Domain.Order.Model;

namespace VirtoCommerce.OrderModule.Data.Services
{
    public interface ICustomerOrderBuilder
    {
        CustomerOrder PlaceCustomerOrderFromCart(ShoppingCart cart);
    }
}
