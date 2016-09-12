using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Cart.Model;
using VirtoCommerce.Domain.Order.Model;

namespace VirtoCommerce.OrderModule.Data.Services
{
    public interface ICustomerOrderBuilder
    {
        CustomerOrder PlaceCustomerOrderFromCart(ShoppingCart cart);
    }
}
