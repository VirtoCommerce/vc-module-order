using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Services
{
    public interface IPaymentFlowService
    {
        Task<OrderPaymentResult> CapturePaymentAsync(OrderPaymentRequest request);
    }
}
