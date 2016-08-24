using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Domain.Shipping.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrderModule.Data.Model
{
    public class ShipmentEntity : OperationEntity
    {
        public ShipmentEntity()
        {
            Items = new NullCollection<ShipmentItemEntity>();
            InPayments = new NullCollection<PaymentInEntity>();
            Discounts = new NullCollection<DiscountEntity>();
            Addresses = new NullCollection<AddressEntity>();
            TaxDetails = new NullCollection<TaxDetailEntity>();
            Packages = new NullCollection<ShipmentPackageEntity>();
        }

        [StringLength(64)]
        public string OrganizationId { get; set; }
        [StringLength(255)]
        public string OrganizationName { get; set; }

        [StringLength(64)]
        public string FulfillmentCenterId { get; set; }
        [StringLength(255)]
        public string FulfillmentCenterName { get; set; }

        [StringLength(64)]
        public string EmployeeId { get; set; }
        [StringLength(255)]
        public string EmployeeName { get; set; }

        [StringLength(64)]
        public string ShipmentMethodCode { get; set; }
        [StringLength(64)]
        public string ShipmentMethodOption { get; set; }

        public decimal? VolumetricWeight { get; set; }
        [StringLength(32)]
        public string WeightUnit { get; set; }
        public decimal? Weight { get; set; }
        [StringLength(32)]
        public string MeasureUnit { get; set; }
        public decimal? Height { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }

        [StringLength(64)]
        public string TaxType { get; set; }

        [Column(TypeName = "Money")]
        public decimal DiscountAmount { get; set; }

        public string CustomerOrderId { get; set; }
        public virtual CustomerOrderEntity CustomerOrder { get; set; }

        public virtual ObservableCollection<ShipmentItemEntity> Items { get; set; }
        public virtual ObservableCollection<ShipmentPackageEntity> Packages { get; set; }
        public virtual ObservableCollection<PaymentInEntity> InPayments { get; set; }
        public virtual ObservableCollection<AddressEntity> Addresses { get; set; }

        public virtual ObservableCollection<DiscountEntity> Discounts { get; set; }
        public virtual ObservableCollection<TaxDetailEntity> TaxDetails { get; set; }


        public override OrderOperation ToModel(OrderOperation operation)
        {
            base.ToModel(operation);

            var shipment = operation as Shipment;
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            if (!this.Addresses.IsNullOrEmpty())
            {
                shipment.DeliveryAddress = this.Addresses.First().ToModel(AbstractTypeFactory<Address>.TryCreateInstance());
            }
            if (!this.Discounts.IsNullOrEmpty())
            {
                shipment.Discount = this.Discounts.First().ToModel(AbstractTypeFactory<Discount>.TryCreateInstance());
            }
            if (!this.Items.IsNullOrEmpty())
            {
                shipment.Items = this.Items.Select(x=> x.ToModel(AbstractTypeFactory<ShipmentItem>.TryCreateInstance())).ToList();
            }
            if (!this.InPayments.IsNullOrEmpty())
            {
                shipment.InPayments = this.InPayments.Select(x => x.ToModel(AbstractTypeFactory<PaymentIn>.TryCreateInstance())).OfType<PaymentIn>().ToList();
            }
            if (!this.Packages.IsNullOrEmpty())
            {
                shipment.Packages = this.Packages.Select(x => x.ToModel(AbstractTypeFactory<ShipmentPackage>.TryCreateInstance())).ToList();
            }
            if (!this.TaxDetails.IsNullOrEmpty())
            {
                shipment.TaxDetails = this.TaxDetails.Select(x => x.ToModel(AbstractTypeFactory<TaxDetail>.TryCreateInstance())).ToList();
            }

            shipment.ChildrenOperations = shipment.GetFlatObjectsListWithInterface<IOperation>().Except(new[] { shipment }).ToList();

            return shipment;
        }

        public override OperationEntity FromModel(OrderOperation operation, PrimaryKeyResolvingMap pkMap)
        {
            base.FromModel(operation, pkMap);

            var shipment = operation as Shipment;
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            this.InjectFrom(shipment);

            pkMap.AddPair(shipment, this);

            //Allow to empty address
            this.Addresses = new ObservableCollection<AddressEntity>();
            if (shipment.DeliveryAddress != null)
            {
                this.Addresses = new ObservableCollection<AddressEntity>(new AddressEntity[] { AbstractTypeFactory<AddressEntity>.TryCreateInstance().FromModel(shipment.DeliveryAddress) });
            }
            if (shipment.Items != null)
            {
                this.Items = new ObservableCollection<ShipmentItemEntity>(shipment.Items.Select(x => AbstractTypeFactory<ShipmentItemEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }
            if (shipment.Packages != null)
            {
                this.Packages = new ObservableCollection<ShipmentPackageEntity>(shipment.Packages.Select(x => AbstractTypeFactory<ShipmentPackageEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }
            if (shipment.TaxDetails != null)
            {
                this.TaxDetails = new ObservableCollection<TaxDetailEntity>(shipment.TaxDetails.Select(x => AbstractTypeFactory<TaxDetailEntity>.TryCreateInstance().FromModel(x)));
            }
            if (shipment.Discount != null)
            {
                this.Discounts = new ObservableCollection<DiscountEntity>(new DiscountEntity[] { AbstractTypeFactory<DiscountEntity>.TryCreateInstance().FromModel(shipment.Discount) });
            }            
            return this;
        }

        public override void Patch(OperationEntity operation)
        {
            base.Patch(operation);

            var target = operation as ShipmentEntity;
            if (target == null)
                throw new ArgumentNullException("target");


            target.FulfillmentCenterId = this.FulfillmentCenterId;
            target.OrganizationId = this.OrganizationId;
            target.EmployeeId = this.EmployeeId;
            target.ShipmentMethodCode = this.ShipmentMethodCode;
            target.ShipmentMethodOption = this.ShipmentMethodOption;
            target.Height = this.Height;
            target.Length = this.Length;
            target.Weight = this.Weight;
            target.Height = this.Height;
            target.Width = this.Width;
            target.MeasureUnit = this.MeasureUnit;
            target.WeightUnit = this.WeightUnit;
            target.Length = this.Length;
            target.TaxType = this.TaxType;
            target.DiscountAmount = this.DiscountAmount;
            

            if (!this.InPayments.IsNullCollection())
            {
                this.InPayments.Patch(target.InPayments, (sourcePayment, targetPayment) => sourcePayment.Patch(targetPayment));
            }
            if (!this.Items.IsNullCollection())
            {
                this.Items.Patch(target.Items, (sourceItem, targetItem) => sourceItem.Patch(targetItem));
            }
            if (!this.Discounts.IsNullCollection())
            {
                var discountComparer = AnonymousComparer.Create((DiscountEntity x) => x.PromotionId);
                this.Discounts.Patch(target.Discounts, discountComparer, (sourceDiscount, targetDiscount) => sourceDiscount.Patch(targetDiscount));
            }
            if (!this.Addresses.IsNullCollection())
            {
                this.Addresses.Patch(target.Addresses, new AddressComparer(), (sourceAddress, targetAddress) => sourceAddress.Patch(targetAddress));
            }
            if (!this.Packages.IsNullCollection())
            {
                this.Packages.Patch(target.Packages, (sourcePackage, targetPackage) => sourcePackage.Patch(targetPackage));
            }
            if (!this.TaxDetails.IsNullCollection())
            {
                var taxDetailComparer = AnonymousComparer.Create((TaxDetailEntity x) => x.Name);
                this.TaxDetails.Patch(target.TaxDetails, taxDetailComparer, (sourceTaxDetail, targetTaxDetail) => sourceTaxDetail.Patch(targetTaxDetail));
            }
        }
    }
}
