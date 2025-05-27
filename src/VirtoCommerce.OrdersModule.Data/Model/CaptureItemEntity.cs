using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.OrdersModule.Data.Model
{
    public class CaptureItemEntity : AuditableEntity, IHasOuterId, IDataEntity<CaptureItemEntity, CaptureItem>
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
            ArgumentNullException.ThrowIfNull(captureItem);

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
            ArgumentNullException.ThrowIfNull(captureItem);

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
            ArgumentNullException.ThrowIfNull(target);

            target.OuterId = OuterId;
            target.CaptureId = CaptureId;
            target.Quantity = Quantity;
        }
    }
}
