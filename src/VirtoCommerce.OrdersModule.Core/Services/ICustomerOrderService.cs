using System.Collections.Specialized;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.OrdersModule.Core.Services
{
    public interface ICustomerOrderService : IOuterEntityService<CustomerOrder>
    {
        Task<PostProcessPaymentRequestResult> PostProcessPaymentAsync(NameValueCollection paymentParameters);
    }
}
