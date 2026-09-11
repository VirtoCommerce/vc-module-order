using System;
using System.Collections.Generic;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Tax;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Extensions;

/// <summary>
/// Discounts, tax details and fee details carry money in their own right. They are value objects and
/// <see cref="Discount"/> belongs to another module, so none of them can reduce itself. Every operation
/// that owns one of these collections has to zero it alongside its own price fields, otherwise the
/// amounts survive redaction and a hidden total can be rebuilt from its own breakdown.
/// </summary>
public static class MonetaryDetailExtensions
{
    public static void RemovePrices(this IEnumerable<Discount> discounts)
    {
        foreach (var discount in discounts ?? Array.Empty<Discount>())
        {
            discount.DiscountAmount = 0m;
            discount.DiscountAmountWithTax = 0m;
        }
    }

    public static void RemovePrices(this IEnumerable<TaxDetail> taxDetails)
    {
        foreach (var taxDetail in taxDetails ?? Array.Empty<TaxDetail>())
        {
            taxDetail.Amount = 0m;
            taxDetail.Rate = 0m;
        }
    }

    public static void RemovePrices(this IEnumerable<FeeDetail> feeDetails)
    {
        foreach (var feeDetail in feeDetails ?? Array.Empty<FeeDetail>())
        {
            feeDetail.Amount = 0m;
        }
    }
}
