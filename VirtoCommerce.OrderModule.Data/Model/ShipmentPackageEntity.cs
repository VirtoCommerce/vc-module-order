using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrderModule.Data.Model
{
	public class ShipmentPackageEntity : AuditableEntity
	{
		public ShipmentPackageEntity()
		{
			Items = new NullCollection<ShipmentItemEntity>();
		}
		[StringLength(128)]
		public string BarCode { get; set; }
		[StringLength(64)]
		public string PackageType { get; set; }

		[StringLength(32)]
		public string WeightUnit { get; set; }
		public decimal? Weight { get; set; }
		[StringLength(32)]
		public string MeasureUnit { get; set; }
		public decimal? Height { get; set; }
		public decimal? Length { get; set; }
		public decimal? Width { get; set; }

		public virtual ShipmentEntity Shipment { get; set; }
		public string ShipmentId { get; set; }

		public virtual ObservableCollection<ShipmentItemEntity> Items { get; set; }

        public virtual ShipmentPackage ToModel(ShipmentPackage package)
        {
            if (package == null)
                throw new ArgumentNullException("package");

            package.InjectFrom(this);

            if (!this.Items.IsNullOrEmpty())
            {
                package.Items = this.Items.Select(x => x.ToModel(AbstractTypeFactory<ShipmentItem>.TryCreateInstance())).ToList();
            }

            return package;
        }

        public virtual ShipmentPackageEntity FromModel(ShipmentPackage package, PrimaryKeyResolvingMap pkMap)
        {
            if (package == null)
                throw new ArgumentNullException("package");

            pkMap.AddPair(package, this);
            this.InjectFrom(package);

            if (!this.Items.IsNullOrEmpty())
            {
                this.Items = new ObservableCollection<ShipmentItemEntity>(package.Items.Select(x => AbstractTypeFactory<ShipmentItemEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }
            return this;
        }

        public virtual void Patch(ShipmentPackageEntity target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            target.PackageType = this.PackageType;
            target.ShipmentId = this.ShipmentId;
            target.Weight = this.Weight;
            target.Height = this.Height;
            target.Width = this.Width;
            target.MeasureUnit = this.MeasureUnit;
            target.WeightUnit = this.WeightUnit;
            target.Length = this.Length;
       
            if (!this.Items.IsNullCollection())
            {
                this.Items.Patch(target.Items, (sourceItem, targetItem) => sourceItem.Patch(targetItem));
            }
        }
    }
}
