using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Data.Model;

public class ConfigurationItemEntity : AuditableEntity
{
    [StringLength(128)]
    public string LineItemId { get; set; }

    public LineItemEntity LineItem { get; set; }

    [StringLength(128)]
    public string ProductId { get; set; }

    [StringLength(1024)]
    public string Name { get; set; }

    [StringLength(128)]
    public string Sku { get; set; }

    public int Quantity { get; set; }

    [StringLength(1028)]
    public string ImageUrl { get; set; }

    [StringLength(128)]
    public string CatalogId { get; set; }

    [StringLength(128)]
    public string CategoryId { get; set; }

    [Required]
    [StringLength(64)]
    public string Type { get; set; }

    [StringLength(255)]
    public string CustomText { get; set; }

    #region Navigation Properties
    public virtual ObservableCollection<ConfigurationItemFileEntity> Files { get; set; } = new NullCollection<ConfigurationItemFileEntity>();
    #endregion

    public virtual ConfigurationItem ToModel(ConfigurationItem configurationItem)
    {
        System.ArgumentNullException.ThrowIfNull(configurationItem);

        configurationItem.Id = Id;
        configurationItem.CreatedBy = CreatedBy;
        configurationItem.CreatedDate = CreatedDate;
        configurationItem.ModifiedBy = ModifiedBy;
        configurationItem.ModifiedDate = ModifiedDate;

        configurationItem.LineItemId = LineItemId;
        configurationItem.ProductId = ProductId;
        configurationItem.Name = Name;
        configurationItem.Sku = Sku;
        configurationItem.Quantity = Quantity;
        configurationItem.ImageUrl = ImageUrl;
        configurationItem.CatalogId = CatalogId;
        configurationItem.CategoryId = CategoryId;
        configurationItem.Type = Type;
        configurationItem.CustomText = CustomText;

        configurationItem.Files = Files.Select(x => x.ToModel(AbstractTypeFactory<ConfigurationItemFile>.TryCreateInstance())).ToList();

        return configurationItem;
    }

    public virtual ConfigurationItemEntity FromModel(ConfigurationItem configurationItem, PrimaryKeyResolvingMap pkMap)
    {
        System.ArgumentNullException.ThrowIfNull(configurationItem);

        pkMap.AddPair(configurationItem, this);

        Id = configurationItem.Id;
        CreatedBy = configurationItem.CreatedBy;
        CreatedDate = configurationItem.CreatedDate;
        ModifiedBy = configurationItem.ModifiedBy;
        ModifiedDate = configurationItem.ModifiedDate;

        LineItemId = configurationItem.LineItemId;
        ProductId = configurationItem.ProductId;
        Name = configurationItem.Name;
        Sku = configurationItem.Sku;
        Quantity = configurationItem.Quantity;
        ImageUrl = configurationItem.ImageUrl;
        CatalogId = configurationItem.CatalogId;
        CategoryId = configurationItem.CategoryId;
        Type = configurationItem.Type;
        CustomText = configurationItem.CustomText;

        if (configurationItem.Files != null)
        {
            Files = new ObservableCollection<ConfigurationItemFileEntity>(configurationItem.Files.Select(x => AbstractTypeFactory<ConfigurationItemFileEntity>.TryCreateInstance().FromModel(x, pkMap)));
        }

        return this;
    }

    public virtual void Patch(ConfigurationItemEntity target)
    {
        target.LineItemId = LineItemId;
        target.ProductId = ProductId;
        target.Name = Name;
        target.Sku = Sku;
        target.Quantity = Quantity;
        target.ImageUrl = ImageUrl;
        target.CatalogId = CatalogId;
        target.CategoryId = CategoryId;
        target.Type = Type;
        target.CustomText = CustomText;

        if (!Files.IsNullCollection())
        {
            Files.Patch(target.Files, (sourceImage, targetImage) => sourceImage.Patch(targetImage));
        }
    }
}
