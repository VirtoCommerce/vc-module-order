using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Core.Model;
public class PurchasedProduct : Entity, ICloneable
{
    public string ProductId { get; set; }

    public string UserId { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
