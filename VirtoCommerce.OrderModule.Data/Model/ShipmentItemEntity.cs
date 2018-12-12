using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrderModule.Data.Model
{
    public class ShipmentItemEntity : AuditableEntity
    {
        [StringLength(128)]
        public string BarCode { get; set; }

        public int Quantity { get; set; }

        public virtual LineItemEntity LineItem { get; set; }
        public string LineItemId { get; set; }

        public virtual ShipmentEntity Shipment { get; set; }
        public string ShipmentId { get; set; }

        public virtual ShipmentPackageEntity ShipmentPackage { get; set; }
        public string ShipmentPackageId { get; set; }

        [NotMapped]
        public LineItem ModelLineItem { get; set; }

        public virtual ShipmentItem ToModel(ShipmentItem shipmentItem)
        {
            if (shipmentItem == null)
                throw new ArgumentNullException(nameof(shipmentItem));

            shipmentItem.InjectFrom(this);

            return shipmentItem;
        }

        public virtual ShipmentItemEntity FromModel(ShipmentItem shipmentItem, PrimaryKeyResolvingMap pkMap)
        {
            if (shipmentItem == null)
                throw new ArgumentNullException(nameof(shipmentItem));

            pkMap.AddPair(shipmentItem, this);
            this.InjectFrom(shipmentItem);
            //Store ModelLineItem for future linking with the order line item only for new objects otherwise we will get error when saving
            if (shipmentItem.LineItem != null)
            {
                LineItemId = shipmentItem.LineItem.Id;
                //Store ModelLineItem for future linking with the order line item only for new objects otherwise we will get error when saving
                if (shipmentItem.LineItem.IsTransient())
                {
                    ModelLineItem = shipmentItem.LineItem;
                }
            }

            return this;
        }

        public virtual void Patch(ShipmentItemEntity target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            target.BarCode = BarCode;
            target.ShipmentId = ShipmentId;
            target.ShipmentPackageId = ShipmentPackageId;
            target.Quantity = Quantity;
        }
    }
}
