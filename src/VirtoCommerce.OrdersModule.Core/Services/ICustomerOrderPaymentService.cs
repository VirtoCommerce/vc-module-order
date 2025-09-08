using System.Collections.Specialized;
using System.Threading.Tasks;
using VirtoCommerce.PaymentModule.Model.Requests;

namespace VirtoCommerce.OrdersModule.Core.Services
{
    public interface ICustomerOrderPaymentService
    {
        Task<PostProcessPaymentRequestResult> PostProcessPaymentAsync(NameValueCollection paymentParameters);
    }
}
