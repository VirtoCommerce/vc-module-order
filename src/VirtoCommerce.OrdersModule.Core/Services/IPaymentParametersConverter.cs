using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Services
{
    public interface IPaymentParametersConverter
    {
        PaymentParameters GetPaymentParameters(PaymentCallbackParameters request);
        PaymentParameters GetPaymentParameters(string requestBody, string requestQuery);
    }
}
