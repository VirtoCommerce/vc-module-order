using System;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Swagger;

namespace VirtoCommerce.OrdersModule.Core.Model;

[SwaggerSchemaId("OrderConfigurationItemFile")]
public class ConfigurationItemFile : AuditableEntity, ICloneable
{
    public string Name { get; set; }
    public string Url { get; set; }
    public string ContentType { get; set; }
    public long Size { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
