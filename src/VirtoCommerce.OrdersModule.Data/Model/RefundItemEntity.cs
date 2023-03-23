using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Data.Model
{
    public class RefundItemEntity : AuditableEntity
    {
        public int Quantity { get; set; }

        #region Navigation Properties

        [NotMapped]
        public LineItem ModelLineItem { get; set; }

        public virtual LineItemEntity LineItem { get; set; }
        public string LineItemId { get; set; }

        public virtual RefundEntity Refund { get; set; }
        public string RefundId { get; set; }

        #endregion

        [StringLength(128)]
        public string OuterId { get; set; }

        public virtual RefundItem ToModel(RefundItem refundItem)
        {
            if (refundItem == null)
            {
                throw new ArgumentNullException(nameof(refundItem));
            }

            refundItem.Id = Id;
            refundItem.CreatedDate = CreatedDate;
            refundItem.CreatedBy = CreatedBy;
            refundItem.ModifiedDate = ModifiedDate;
            refundItem.ModifiedBy = ModifiedBy;
            refundItem.OuterId = OuterId;

            refundItem.Quantity = Quantity;

            refundItem.LineItemId = LineItemId;

            if (ModelLineItem != null)
            {
                refundItem.LineItem = ModelLineItem;
            }

            return refundItem;
        }

        public virtual RefundItemEntity FromModel(RefundItem refundItem, PrimaryKeyResolvingMap pkMap)
        {
            if (refundItem == null)
            {
                throw new ArgumentNullException(nameof(refundItem));
            }

            Id = refundItem.Id;
            CreatedDate = refundItem.CreatedDate;
            CreatedBy = refundItem.CreatedBy;
            ModifiedDate = refundItem.ModifiedDate;
            ModifiedBy = refundItem.ModifiedBy;
            OuterId = refundItem.OuterId;

            Quantity = refundItem.Quantity;

            pkMap.AddPair(refundItem, this);

            if (refundItem.LineItem != null)
            {
                LineItemId = refundItem.LineItem.Id;
                ModelLineItem = refundItem.LineItem;
            }

            return this;
        }

        public virtual void Patch(RefundItemEntity target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            target.RefundId = RefundId;
            target.Quantity = Quantity;
        }
    }
}
