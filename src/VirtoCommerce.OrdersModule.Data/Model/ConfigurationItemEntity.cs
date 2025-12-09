using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.OrdersModule.Data.Model;

public class ConfigurationItemEntity : AuditableEntity, IDataEntity<ConfigurationItemEntity, ConfigurationItem>
{
    [StringLength(128)]
    public string LineItemId { get; set; }

    public LineItemEntity LineItem { get; set; }

    [StringLength(128)]
    public string ProductId { get; set; }

    [StringLength(128)]
    public string SectionId { get; set; }

    [StringLength(1024)]
    public string Name { get; set; }

    [StringLength(128)]
    public string Sku { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "Money")]
    public decimal Price { get; set; }

    [Column(TypeName = "Money")]
    public decimal SalePrice { get; set; }

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

    public string ProductSnapshot { get; set; }

    #region Navigation Properties
    public virtual ObservableCollection<ConfigurationItemFileEntity> Files { get; set; } = new NullCollection<ConfigurationItemFileEntity>();
    #endregion

    public virtual ConfigurationItem ToModel(ConfigurationItem configurationItem)
    {
        ArgumentNullException.ThrowIfNull(configurationItem);

        configurationItem.Id = Id;
        configurationItem.CreatedBy = CreatedBy;
        configurationItem.CreatedDate = CreatedDate;
        configurationItem.ModifiedBy = ModifiedBy;
        configurationItem.ModifiedDate = ModifiedDate;

        configurationItem.ProductId = ProductId;
        configurationItem.SectionId = SectionId;
        configurationItem.Name = Name;
        configurationItem.Sku = Sku;
        configurationItem.Quantity = Quantity;
        configurationItem.Price = Price;
        configurationItem.SalePrice = SalePrice;
        configurationItem.ImageUrl = ImageUrl;
        configurationItem.CatalogId = CatalogId;
        configurationItem.CategoryId = CategoryId;
        configurationItem.Type = Type;
        configurationItem.CustomText = CustomText;
        configurationItem.ProductSnapshot = ProductSnapshot;

        configurationItem.Files = Files.Select(x => x.ToModel(AbstractTypeFactory<ConfigurationItemFile>.TryCreateInstance())).ToList();

        return configurationItem;
    }

    public virtual ConfigurationItemEntity FromModel(ConfigurationItem configurationItem, PrimaryKeyResolvingMap pkMap)
    {
        ArgumentNullException.ThrowIfNull(configurationItem);

        pkMap.AddPair(configurationItem, this);

        Id = configurationItem.Id;
        CreatedBy = configurationItem.CreatedBy;
        CreatedDate = configurationItem.CreatedDate;
        ModifiedBy = configurationItem.ModifiedBy;
        ModifiedDate = configurationItem.ModifiedDate;

        ProductId = configurationItem.ProductId;
        SectionId = configurationItem.SectionId;
        Name = configurationItem.Name;
        Sku = configurationItem.Sku;
        Quantity = configurationItem.Quantity;
        Price = configurationItem.Price;
        SalePrice = configurationItem.SalePrice;
        ImageUrl = configurationItem.ImageUrl;
        CatalogId = configurationItem.CatalogId;
        CategoryId = configurationItem.CategoryId;
        Type = configurationItem.Type;
        CustomText = configurationItem.CustomText;
        ProductSnapshot = configurationItem.ProductSnapshot;

        if (configurationItem.Files != null)
        {
            Files = new ObservableCollection<ConfigurationItemFileEntity>(configurationItem.Files.Select(x => AbstractTypeFactory<ConfigurationItemFileEntity>.TryCreateInstance().FromModel(x, pkMap)));
        }

        return this;
    }

    public virtual void Patch(ConfigurationItemEntity target)
    {
        ArgumentNullException.ThrowIfNull(target);

        target.ProductId = ProductId;
        target.SectionId = SectionId;
        target.Name = Name;
        target.Sku = Sku;
        target.Quantity = Quantity;
        target.Price = Price;
        target.SalePrice = SalePrice;
        target.ImageUrl = ImageUrl;
        target.CatalogId = CatalogId;
        target.CategoryId = CategoryId;
        target.Type = Type;
        target.CustomText = CustomText;
        target.ProductSnapshot = ProductSnapshot;

        if (!Files.IsNullCollection())
        {
            Files.Patch(target.Files, (sourceImage, targetImage) => sourceImage.Patch(targetImage));
        }
    }
}
