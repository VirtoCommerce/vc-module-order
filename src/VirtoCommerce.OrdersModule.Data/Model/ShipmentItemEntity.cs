using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.OrdersModule.Data.Model
{
    public class ShipmentItemEntity : AuditableEntity, IHasOuterId, IDataEntity<ShipmentItemEntity, ShipmentItem>
    {
        [StringLength(128)]
        public string BarCode { get; set; }

        [StringLength(128)]
        public string Status { get; set; }

        public int Quantity { get; set; }

        #region Navigation Properties

        [NotMapped]
        public LineItem ModelLineItem { get; set; }

        public virtual LineItemEntity LineItem { get; set; }
        public string LineItemId { get; set; }

        public virtual ShipmentEntity Shipment { get; set; }
        public string ShipmentId { get; set; }

        public virtual ShipmentPackageEntity ShipmentPackage { get; set; }
        public string ShipmentPackageId { get; set; }

        #endregion

        [StringLength(128)]
        public string OuterId { get; set; }


        public virtual ShipmentItem ToModel(ShipmentItem shipmentItem)
        {
            ArgumentNullException.ThrowIfNull(shipmentItem);

            shipmentItem.Id = Id;
            shipmentItem.CreatedDate = CreatedDate;
            shipmentItem.CreatedBy = CreatedBy;
            shipmentItem.ModifiedDate = ModifiedDate;
            shipmentItem.ModifiedBy = ModifiedBy;
            shipmentItem.OuterId = OuterId;

            shipmentItem.BarCode = BarCode;
            shipmentItem.Status = Status;
            shipmentItem.Quantity = Quantity;

            shipmentItem.LineItemId = LineItemId;

            if (ModelLineItem != null)
            {
                shipmentItem.LineItem = ModelLineItem;
            }

            return shipmentItem;
        }

        public virtual ShipmentItemEntity FromModel(ShipmentItem shipmentItem, PrimaryKeyResolvingMap pkMap)
        {
            ArgumentNullException.ThrowIfNull(shipmentItem);

            Id = shipmentItem.Id;
            CreatedDate = shipmentItem.CreatedDate;
            CreatedBy = shipmentItem.CreatedBy;
            ModifiedDate = shipmentItem.ModifiedDate;
            ModifiedBy = shipmentItem.ModifiedBy;
            OuterId = shipmentItem.OuterId;

            BarCode = shipmentItem.BarCode;
            Status = shipmentItem.Status;
            Quantity = shipmentItem.Quantity;

            pkMap.AddPair(shipmentItem, this);
            if (shipmentItem.LineItem != null)
            {
                LineItemId = shipmentItem.LineItem.Id;
                ModelLineItem = shipmentItem.LineItem;
            }

            return this;
        }

        public virtual void Patch(ShipmentItemEntity target)
        {
            ArgumentNullException.ThrowIfNull(target);

            target.OuterId = OuterId;
            target.BarCode = BarCode;
            target.Status = Status;
            target.ShipmentId = ShipmentId;
            target.ShipmentPackageId = ShipmentPackageId;
            target.Quantity = Quantity;
        }
    }
}
