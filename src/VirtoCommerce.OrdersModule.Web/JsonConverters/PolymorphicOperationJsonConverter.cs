using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CoreModule.Core.Common;

using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.OrdersModule.Data.Handlers;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.JsonConverters;
using VirtoCommerce.ShippingModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Web.JsonConverters
{
    public class PolymorphicOperationJsonConverter : PolymorphJsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IOperation).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object retVal;
            var obj = JObject.Load(reader);

            //Reset ChildrenOperations property to prevent polymorphic deserialization  error
            obj.Remove("childrenOperations");
            obj.Remove("ChildrenOperations");

            if (obj.ContainsKey("paymentStatus") || obj.ContainsKey("PaymentStatus"))
            {
                var paymentStatus = (obj["paymentStatus"] ?? obj["PaymentStatus"]).Value<string>();
                var hasStatusValue = Enum.IsDefined(typeof(PaymentStatus), paymentStatus);
                if (!hasStatusValue)
                {
                    obj["paymentStatus"] = PaymentStatus.Custom.ToString();
                }
            }

            return base.ReadJson(obj.CreateReader(), objectType, existingValue, serializer);
        }
    }
}
