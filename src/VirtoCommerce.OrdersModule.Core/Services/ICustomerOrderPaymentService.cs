using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;

namespace VirtoCommerce.OrdersModule.Core.Services
{
    public interface ICustomerOrderPaymentService
    {
        Task<PostProcessPaymentRequestResult> PostProcessPaymentAsync(PaymentParameters paymentParameters);
    }
}
