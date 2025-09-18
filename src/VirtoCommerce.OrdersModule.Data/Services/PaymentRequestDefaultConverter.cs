using Newtonsoft.Json;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Data.Services;

public class PaymentRequestDefaultConverter : IPaymentRequestConverter
{
    public virtual PaymentParameters GetPaymentParameters(PaymentCallbackParameters request)
    {
        var result = AbstractTypeFactory<PaymentParameters>.TryCreateInstance();

        foreach (var parameter in request?.Parameters ?? [])
        {
            result.Parameters.Add(parameter.Key, parameter.Value);
        }

        result.OrderId = result.Parameters.Get("orderid");
        result.PaymentMethodCode = result.Parameters.Get("code");

        return result;
    }

    public virtual PaymentParameters GetPaymentParameters(string requestBody, string requestQuery)
    {
        var paymentCallbackParameters = JsonConvert.DeserializeObject<PaymentCallbackParameters>(requestBody);
        return GetPaymentParameters(paymentCallbackParameters);
    }

    public virtual (object, bool) GetResponse(PostProcessPaymentRequestResult result)
    {
        if (result is PostProcessPaymentRequestNotValidResult notValidResult)
        {
            var response = new
            {
                Message = notValidResult.ErrorMessage,
                Errors = notValidResult.Errors,
            };

            return (response, false);
        }

        return (result, true);
    }
}
