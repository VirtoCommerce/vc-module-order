using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;

namespace VirtoCommerce.OrdersModule.Core.Services
{
    public interface IPaymentRequestConverter
    {
        PaymentParameters GetPaymentParameters(PaymentCallbackParameters request);
        PaymentParameters GetPaymentParameters(string requestBody, string requestQuery);
        bool IsFailure(PostProcessPaymentRequestResult result);
        object GetResponse(PostProcessPaymentRequestResult result);
    }
}
