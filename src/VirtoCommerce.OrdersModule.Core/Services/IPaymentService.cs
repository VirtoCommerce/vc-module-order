using System;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Services
{
    public interface IPaymentService
    {
        Task<PaymentIn[]> GetByIdsAsync(string[] ids, string responseGroup = null);
        Task<PaymentIn> GetByIdAsync(string ids, string responseGroup = null);
        Task SaveChangesAsync(PaymentIn[] payments);
        Task DeleteAsync(string[] ids);
    }
}
