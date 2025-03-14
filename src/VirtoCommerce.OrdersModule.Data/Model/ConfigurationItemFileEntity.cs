using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.OrdersModule.Data.Model;

public class ConfigurationItemFileEntity : AuditableEntity, IDataEntity<ConfigurationItemFileEntity, ConfigurationItemFile>
{
    [StringLength(2083)]
    [Required]
    public string Url { get; set; }

    [StringLength(1024)]
    public string Name { get; set; }

    [StringLength(128)]
    public string ContentType { get; set; }

    public long Size { get; set; }

    [Required]
    [StringLength(128)]
    public string ConfigurationItemId { get; set; }

    #region Navigation Properties

    public virtual ConfigurationItemEntity ConfigurationItem { get; set; }

    #endregion

    public ConfigurationItemFile ToModel(ConfigurationItemFile model)
    {
        ArgumentNullException.ThrowIfNull(model);

        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.Name = Name;
        model.Url = Url;
        model.ContentType = ContentType;
        model.Size = Size;
        model.ConfigurationItemId = ConfigurationItemId;

        return model;
    }

    public ConfigurationItemFileEntity FromModel(ConfigurationItemFile model, PrimaryKeyResolvingMap pkMap)
    {
        ArgumentNullException.ThrowIfNull(model);

        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        Name = model.Name;
        Url = model.Url;
        ContentType = model.ContentType;
        Size = model.Size;
        ConfigurationItemId = model.ConfigurationItemId;

        return this;
    }

    public void Patch(ConfigurationItemFileEntity target)
    {
        target.Name = Name;
        target.Url = Url;
        target.ContentType = ContentType;
        target.Size = Size;
    }
}
