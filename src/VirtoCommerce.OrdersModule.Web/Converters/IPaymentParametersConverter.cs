using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Web.Model;

namespace VirtoCommerce.OrdersModule.Web.Converters;

public interface IPaymentParametersConverter
{
    PaymentParameters GetPaymentParameters(PaymentCallbackParameters request);
    PaymentParameters GetPaymentParameters(string requestBody, string requestQuery);
}
