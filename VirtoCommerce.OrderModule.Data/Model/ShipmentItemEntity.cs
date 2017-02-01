using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                throw new ArgumentNullException("shipmentItem");

            shipmentItem.InjectFrom(this);
    
            return shipmentItem;
        }

        public virtual ShipmentItemEntity FromModel(ShipmentItem shipmentItem, PrimaryKeyResolvingMap pkMap)
        {
            if (shipmentItem == null)
                throw new ArgumentNullException("shipmentItem");


            pkMap.AddPair(shipmentItem, this);
            this.InjectFrom(shipmentItem);
            this.ModelLineItem = shipmentItem.LineItem;
            return this;
        }

        public virtual void Patch(ShipmentItemEntity target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            target.BarCode = this.BarCode;
            target.ShipmentId = this.ShipmentId;
            target.ShipmentPackageId = this.ShipmentPackageId;
            target.Quantity = this.Quantity;
        }

    }
}
