using System;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Services
{
    public interface ICustomerOrderService
    {
        [Obsolete(@"Need to remove after inherit ICustomerOrderService from ICrudService<CustomerOrder>")]
        Task<CustomerOrder[]> GetByIdsAsync(string[] orderIds, string responseGroup = null);
        [Obsolete(@"Need to remove after inherit ICustomerOrderService from ICrudService<CustomerOrder>")]
        Task<CustomerOrder> GetByIdAsync(string orderId, string responseGroup = null);
        Task SaveChangesAsync(CustomerOrder[] orders);
        Task DeleteAsync(string[] ids);
    }
}
