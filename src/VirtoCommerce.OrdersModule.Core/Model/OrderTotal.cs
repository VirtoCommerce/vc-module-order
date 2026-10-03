using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Core.Model;

public class OrderTotal : Entity, ICloneable
{
    public string CurrencyCode { get; set; }

    public decimal Total { get; set; }

    public decimal SubTotal { get; set; }

    public decimal TaxTotal { get; set; }

    public decimal DiscountTotal { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
