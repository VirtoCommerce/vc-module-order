using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Data.Model
{
    public class CaptureItemEntity : AuditableEntity
    {
        public int Quantity { get; set; }

        #region Navigation Properties

        [NotMapped]
        public LineItem ModelLineItem { get; set; }

        public virtual LineItemEntity LineItem { get; set; }
        public string LineItemId { get; set; }

        public virtual CaptureEntity Capture { get; set; }
        public string CaptureId { get; set; }

        #endregion

        [StringLength(128)]
        public string OuterId { get; set; }

        public virtual CaptureItem ToModel(CaptureItem captureItem)
        {
            if (captureItem == null)
            {
                throw new ArgumentNullException(nameof(captureItem));
            }

            captureItem.Id = Id;
            captureItem.CreatedDate = CreatedDate;
            captureItem.CreatedBy = CreatedBy;
            captureItem.ModifiedDate = ModifiedDate;
            captureItem.ModifiedBy = ModifiedBy;
            captureItem.OuterId = OuterId;

            captureItem.Quantity = Quantity;

            captureItem.LineItemId = LineItemId;

            if (ModelLineItem != null)
            {
                captureItem.LineItem = ModelLineItem;
            }

            return captureItem;
        }

        public virtual CaptureItemEntity FromModel(CaptureItem captureItem, PrimaryKeyResolvingMap pkMap)
        {
            if (captureItem == null)
            {
                throw new ArgumentNullException(nameof(captureItem));
            }

            Id = captureItem.Id;
            CreatedDate = captureItem.CreatedDate;
            CreatedBy = captureItem.CreatedBy;
            ModifiedDate = captureItem.ModifiedDate;
            ModifiedBy = captureItem.ModifiedBy;
            OuterId = captureItem.OuterId;

            Quantity = captureItem.Quantity;

            pkMap.AddPair(captureItem, this);

            if (captureItem.LineItem != null)
            {
                LineItemId = captureItem.LineItem.Id;
                ModelLineItem = captureItem.LineItem;
            }

            return this;
        }

        public virtual void Patch(CaptureItemEntity target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            target.CaptureId = CaptureId;
            target.Quantity = Quantity;
        }
    }
}
