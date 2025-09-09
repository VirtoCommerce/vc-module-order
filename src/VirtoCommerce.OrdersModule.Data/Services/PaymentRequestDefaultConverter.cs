using System;
using System.Collections.Specialized;
using Newtonsoft.Json;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.PaymentModule.Model.Requests;

namespace VirtoCommerce.OrdersModule.Data.Services
{
    public class PaymentRequestDefaultConverter : IPaymentRequestConverter
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

        public virtual bool IsFailure(PostProcessPaymentRequestResult result)
        {
            return result is PostProcessPaymentRequestNotValidResult;
        }

        public virtual object GetResponse(PostProcessPaymentRequestResult result)
        {
            if (IsFailure(result))
            {
                return new
                {
                    Message = (result as PostProcessPaymentRequestNotValidResult)?.ErrorMessage,
                    Errors = (result as PostProcessPaymentRequestNotValidResult)?.Errors
                };
            }
            else
            {
                return result;
            }
        }
    }
}
