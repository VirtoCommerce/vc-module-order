using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Domain.Payment.Model;
using VirtoCommerce.Domain.Payment.Services;
using VirtoCommerce.Domain.Shipping.Model;
using VirtoCommerce.Domain.Shipping.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrderModule.Web.JsonConverters
{
    public class PolymorphicOperationJsonConverter : JsonConverter
    {
        private static Type[] _knowTypes = new[] { typeof(IOperation), typeof(LineItem), typeof(CustomerOrderSearchCriteria), typeof(PaymentMethod), typeof(ShippingMethod) };

        private readonly IPaymentMethodsService _paymentMethodsService;
        private readonly IShippingMethodsService _shippingMethodsService;
        public PolymorphicOperationJsonConverter(IPaymentMethodsService paymentMethodsService, IShippingMethodsService shippingMethodsService)
        {
            _paymentMethodsService = paymentMethodsService;
            _shippingMethodsService = shippingMethodsService;
        }

        public override bool CanWrite { get { return false; } }
        public override bool CanRead { get { return true; } }

        public override bool CanConvert(Type objectType)
        {
            return _knowTypes.Any(x => x.IsAssignableFrom(objectType));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object retVal = null;
            var obj = JObject.Load(reader);
          
            if (typeof(CustomerOrder).IsAssignableFrom(objectType))
            {
                retVal = AbstractTypeFactory<CustomerOrder>.TryCreateInstance();
            }      
            else if (typeof(LineItem).IsAssignableFrom(objectType))
            {
                retVal = AbstractTypeFactory<LineItem>.TryCreateInstance();
            }
            else if (typeof(Shipment).IsAssignableFrom(objectType))
            {
                retVal = AbstractTypeFactory<Shipment>.TryCreateInstance();
            }
            else if (typeof(PaymentIn).IsAssignableFrom(objectType))
            {
                retVal = AbstractTypeFactory<PaymentIn>.TryCreateInstance();
            }
            else if (typeof(CustomerOrderSearchCriteria).IsAssignableFrom(objectType))
            {
                retVal = AbstractTypeFactory<CustomerOrderSearchCriteria>.TryCreateInstance();
            }
            else if(objectType == typeof(PaymentMethod))
            {
                var paymentGatewayCode = obj["code"].Value<string>();
                retVal = _paymentMethodsService.GetAllPaymentMethods().FirstOrDefault(x => x.Code.EqualsInvariant(paymentGatewayCode));
            }
            else if(objectType == typeof(ShippingMethod))
            {
                var shippingGatewayCode = obj["code"].Value<string>();
                retVal = _shippingMethodsService.GetAllShippingMethods().FirstOrDefault(x => x.Code.EqualsInvariant(shippingGatewayCode));
            }
            else if (typeof(IOperation).IsAssignableFrom(objectType))
            {
                var pt = obj["operationType"];
                if (pt != null)
                {
                    var operationType = pt.Value<string>();
                    retVal = AbstractTypeFactory<IOperation>.TryCreateInstance(operationType);
                    if (retVal == null)
                    {
                        throw new NotSupportedException("Unknown operation type: " + operationType);
                    }
                }                
            }
            serializer.Populate(obj.CreateReader(), retVal);
            return retVal;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}