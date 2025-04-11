using System.ComponentModel.DataAnnotations;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.OrdersModule.Data.Model;
public class PurchasedProductEntity : Entity, IDataEntity<PurchasedProductEntity, PurchasedProduct>
{
    [StringLength(128)]
    public string ProductId { get; set; }

    [StringLength(128)]
    public string UserId { get; set; }

    public PurchasedProductEntity FromModel(PurchasedProduct model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.Id;

        ProductId = model.ProductId;
        UserId = model.UserId;

        return this;
    }

    public PurchasedProduct ToModel(PurchasedProduct model)
    {
        model.Id = Id;

        model.ProductId = ProductId;
        model.UserId = UserId;

        return model;
    }

    public void Patch(PurchasedProductEntity target)
    {
        target.ProductId = ProductId;
        target.UserId = UserId;
    }
}

