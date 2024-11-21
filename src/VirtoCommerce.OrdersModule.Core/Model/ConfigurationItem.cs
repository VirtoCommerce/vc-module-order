using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Swagger;

namespace VirtoCommerce.OrdersModule.Core.Model;

[SwaggerSchemaId("OrderConfigurationItem")]
public class ConfigurationItem : AuditableEntity
{
    public string LineItemId { get; set; }

    public string ProductId { get; set; }

    public string Name { get; set; }

    public string Sku { get; set; }

    public int Quantity { get; set; }

    public string ImageUrl { get; set; }

    public string CatalogId { get; set; }

    public string CategoryId { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
