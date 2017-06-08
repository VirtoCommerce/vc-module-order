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
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrderModule.Data.Model
{
	public class CustomerOrderEntity : OperationEntity
	{
		public CustomerOrderEntity()
		{
			Addresses = new NullCollection<AddressEntity>();
			InPayments = new NullCollection<PaymentInEntity>();
			Items = new NullCollection<LineItemEntity>();
			Shipments = new NullCollection<ShipmentEntity>();
			Discounts = new NullCollection<DiscountEntity>();
			TaxDetails = new NullCollection<TaxDetailEntity>();
		}

		[Required]
		[StringLength(64)]
		public string CustomerId { get; set; }
		[StringLength(255)]
		public string CustomerName { get; set; }

		[Required]
		[StringLength(64)]
		public string StoreId { get; set; }
		[StringLength(255)]
		public string StoreName { get; set; }

		[StringLength(64)]
		public string ChannelId { get; set; }
		[StringLength(64)]
		public string OrganizationId { get; set; }
		[StringLength(255)]
		public string OrganizationName { get; set; }

		[StringLength(64)]
		public string EmployeeId { get; set; }
		[StringLength(255)]
		public string EmployeeName { get; set; }

        [StringLength(64)]
        public string SubscriptionId { get; set; }
        [StringLength(64)]
        public string SubscriptionNumber { get; set; }

        public bool IsPrototype { get; set; }

        [Column(TypeName = "Money")]
        public decimal DiscountAmount { get; set; }
        [Column(TypeName = "Money")]
        public decimal TaxTotal { get; set; }
        [Column(TypeName = "Money")]
        public decimal Total { get; set; }
        [Column(TypeName = "Money")]
        public decimal SubTotal { get; set; }
        [Column(TypeName = "Money")]
        public decimal SubTotalWithTax { get; set; }
        [Column(TypeName = "Money")]
        public decimal ShippingTotal { get; set; }
        [Column(TypeName = "Money")]
        public decimal ShippingTotalWithTax { get; set; }
        [Column(TypeName = "Money")]
        public decimal PaymentTotal { get; set; }
        [Column(TypeName = "Money")]
        public decimal PaymentTotalWithTax { get; set; }
        [Column(TypeName = "Money")]
        public decimal HandlingTotal { get; set; }
        [Column(TypeName = "Money")]
        public decimal HandlingTotalWithTax { get; set; }
        [Column(TypeName = "Money")]
        public decimal DiscountTotal { get; set; }
        [Column(TypeName = "Money")]
        public decimal DiscountTotalWithTax { get; set; }
        [StringLength(16)]
        public string LanguageCode { get; set; }
        public decimal TaxPercentRate { get; set; }

        [StringLength(128)]
        public string ShoppingCartId { get; set; }

        public virtual ObservableCollection<TaxDetailEntity> TaxDetails { get; set; }
		public virtual ObservableCollection<AddressEntity> Addresses { get; set; }
		public virtual ObservableCollection<PaymentInEntity> InPayments { get; set; }

		public virtual ObservableCollection<LineItemEntity> Items { get; set; }
		public virtual ObservableCollection<ShipmentEntity> Shipments { get; set; }

		public virtual ObservableCollection<DiscountEntity> Discounts { get; set; }

        public override OrderOperation ToModel(OrderOperation operation)
        {          
            var order = operation as CustomerOrder;
            if (order == null)
                throw new NullReferenceException("order");
            
            order.Discounts = this.Discounts.Select(x => x.ToModel(AbstractTypeFactory<Discount>.TryCreateInstance())).ToList();
            order.Items = this.Items.Select(x => x.ToModel(AbstractTypeFactory<LineItem>.TryCreateInstance())).ToList();
            order.Addresses = this.Addresses.Select(x => x.ToModel(AbstractTypeFactory<Address>.TryCreateInstance())).ToList();
            order.Shipments = this.Shipments.Select(x => x.ToModel(AbstractTypeFactory<Shipment>.TryCreateInstance())).OfType<Shipment>().ToList();
            order.InPayments = this.InPayments.Select(x => x.ToModel(AbstractTypeFactory<PaymentIn>.TryCreateInstance())).OfType<PaymentIn>().ToList();
            order.TaxDetails = this.TaxDetails.Select(x => x.ToModel(AbstractTypeFactory<TaxDetail>.TryCreateInstance())).ToList();

            base.ToModel(order);

            this.Sum = order.Total;

            return order;
        }

        public override OperationEntity FromModel(OrderOperation operation, PrimaryKeyResolvingMap pkMap)
        {         
            var order = operation as CustomerOrder;
            if (order == null)
                throw new NullReferenceException("order");

            base.FromModel(order, pkMap);

            if (order.Addresses != null)
            {
                this.Addresses = new ObservableCollection<AddressEntity>(order.Addresses.Select(x => AbstractTypeFactory<AddressEntity>.TryCreateInstance().FromModel(x)));
            }
            if (order.Items != null)
            {
                this.Items = new ObservableCollection<LineItemEntity>(order.Items.Select(x => AbstractTypeFactory<LineItemEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }
            if (order.Shipments != null)
            {
                this.Shipments = new ObservableCollection<ShipmentEntity>(order.Shipments.Select(x => AbstractTypeFactory<ShipmentEntity>.TryCreateInstance().FromModel(x, pkMap)).OfType<ShipmentEntity>());
                //Link shipment item with order lineItem 
                foreach (var shipmentItemEntity in this.Shipments.SelectMany(x => x.Items))
                {
                    shipmentItemEntity.LineItem = this.Items.FirstOrDefault(x => x.ModelLineItem == shipmentItemEntity.ModelLineItem);
                }
            }
            if (order.InPayments != null)
            {
                this.InPayments = new ObservableCollection<PaymentInEntity>(order.InPayments.Select(x => AbstractTypeFactory<PaymentInEntity>.TryCreateInstance().FromModel(x, pkMap)).OfType<PaymentInEntity>());
            }
            if (order.Discounts != null)
            {
                this.Discounts = new ObservableCollection<DiscountEntity>(order.Discounts.Select(x => AbstractTypeFactory<DiscountEntity>.TryCreateInstance().FromModel(x)));
            }
            if (order.TaxDetails != null)
            {
                this.TaxDetails = new ObservableCollection<TaxDetailEntity>(order.TaxDetails.Select(x => AbstractTypeFactory<TaxDetailEntity>.TryCreateInstance().FromModel(x))); 
            }
            this.Sum = order.Total;
            return this;
        }

        public override void Patch(OperationEntity operation)
        {
            var target = operation as CustomerOrderEntity;
            if (target == null)
                throw new NullReferenceException("target");

            target.CustomerId = this.CustomerId;
            target.CustomerName = this.CustomerName;
            target.StoreId = this.StoreId;
            target.OrganizationId = this.OrganizationId;
            target.EmployeeId = this.EmployeeId;
            target.DiscountAmount = this.DiscountAmount;
            target.Total = this.Total;
            target.SubTotal = this.SubTotal;
            target.SubTotalWithTax = this.SubTotalWithTax;
            target.ShippingTotal = this.ShippingTotal;
            target.ShippingTotalWithTax = this.ShippingTotalWithTax;
            target.PaymentTotal = this.PaymentTotal;
            target.PaymentTotalWithTax = this.PaymentTotalWithTax;
            target.HandlingTotal = this.HandlingTotal;
            target.HandlingTotalWithTax = this.HandlingTotalWithTax;
            target.DiscountTotal = this.DiscountTotal;
            target.DiscountTotalWithTax = this.DiscountTotalWithTax;
            target.DiscountAmount = this.DiscountAmount;
            target.TaxTotal = this.TaxTotal;
            target.IsPrototype = this.IsPrototype;
            target.SubscriptionNumber = this.SubscriptionNumber;
            target.SubscriptionId = this.SubscriptionId;
            target.LanguageCode = this.LanguageCode;
            target.TaxPercentRate = this.TaxPercentRate;

            if (!this.Addresses.IsNullCollection())
            {
                this.Addresses.Patch(target.Addresses, new AddressComparer(), (sourceItem, targetItem) => sourceItem.Patch(targetItem));
            }

            if (!this.Shipments.IsNullCollection())
            {
                this.Shipments.Patch(target.Shipments, (sourceShipment, targetShipment) => sourceShipment.Patch(targetShipment));
            }

            if (!this.Items.IsNullCollection())
            {
                this.Items.Patch(target.Items, (sourceItem, targetItem) => sourceItem.Patch(targetItem));
            }

            if (!this.InPayments.IsNullCollection())
            {
                this.InPayments.Patch(target.InPayments, (sourcePayment, targetPayment) => sourcePayment.Patch(targetPayment));
            }

            if (!this.Discounts.IsNullCollection())
            {
                var discountComparer = AnonymousComparer.Create((DiscountEntity x) => x.PromotionId);
                this.Discounts.Patch(target.Discounts, discountComparer, (sourceDiscount, targetDiscount) => sourceDiscount.Patch(targetDiscount));
            }
            if (!this.TaxDetails.IsNullCollection())
            {
                var taxDetailComparer = AnonymousComparer.Create((TaxDetailEntity x) => x.Name);
                this.TaxDetails.Patch(target.TaxDetails, taxDetailComparer, (sourceTaxDetail, targetTaxDetail) => sourceTaxDetail.Patch(targetTaxDetail));
            }

            base.Patch(operation);
        }
    }
}
