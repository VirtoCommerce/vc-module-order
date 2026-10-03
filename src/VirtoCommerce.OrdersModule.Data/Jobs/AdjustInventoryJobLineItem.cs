using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Data.Jobs
{
    /// <summary>
    /// The part of an ordered line item that the inventory adjustment job needs.
    /// </summary>
    public class AdjustInventoryJobLineItem
    {
        /// <summary>Line item id. The reservation is keyed by it, and the old and new items are matched by it.</summary>
        public string Id { get; set; }

        /// <summary>Product the line item was ordered for.</summary>
        public string ProductId { get; set; }

        /// <summary>Ordered quantity.</summary>
        public int Quantity { get; set; }

        public static AdjustInventoryJobLineItem FromLineItem(LineItem lineItem)
        {
            var result = AbstractTypeFactory<AdjustInventoryJobLineItem>.TryCreateInstance();

            result.Id = lineItem.Id;
            result.ProductId = lineItem.ProductId;
            result.Quantity = lineItem.Quantity;

            return result;
        }

        public virtual LineItem ToLineItem()
        {
            var result = AbstractTypeFactory<LineItem>.TryCreateInstance();

            result.Id = Id;
            result.ProductId = ProductId;
            result.Quantity = Quantity;

            return result;
        }
    }
}
