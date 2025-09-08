using System;
using System.Collections.Specialized;
using Newtonsoft.Json;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Web.Model;

namespace VirtoCommerce.OrdersModule.Web.Converters;

public class PaymentParametersDefaultConverter : IPaymentParametersConverter
{
    public virtual PaymentParameters GetPaymentParameters(PaymentCallbackParameters request)
    {
        var result = new PaymentParameters();

        result.Parameters = new NameValueCollection();
        foreach (var parameter in request?.Parameters ?? Array.Empty<KeyValuePair>())
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
}
