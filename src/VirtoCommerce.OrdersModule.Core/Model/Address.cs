using System.Collections.Generic;
using System.Reflection;
using VirtoCommerce.Platform.Core.Swagger;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    [SwaggerSchemaId("OrderAddress")]
    public class Address : CoreModule.Core.Common.Address
    {
        public virtual IEnumerable<PropertyInfo> GetAllProperties()
        {
            return GetProperties();
        }
    }
}
