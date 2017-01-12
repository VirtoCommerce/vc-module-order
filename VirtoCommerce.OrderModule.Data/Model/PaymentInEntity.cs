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
using VirtoCommerce.Domain.Payment.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrderModule.Data.Model
{
	public class PaymentInEntity : OperationEntity
	{
		public PaymentInEntity()
		{
			Addresses = new NullCollection<AddressEntity>();
            TaxDetails = new NullCollection<TaxDetailEntity>();
            Discounts = new NullCollection<DiscountEntity>();
        }
		[StringLength(64)]
		public string OrganizationId { get; set; }
		[StringLength(255)]
		public string OrganizationName { get; set; }

		[Required]
		[StringLength(64)]
		public string CustomerId { get; set; }
		[StringLength(255)]
		public string CustomerName { get; set; }

		public DateTime? IncomingDate { get; set; }
		[StringLength(128)]
		public string OuterId { get; set; }
		[StringLength(1024)]
		public string Purpose { get; set; }
		[StringLength(64)]
		public string GatewayCode { get; set; }

		public DateTime? AuthorizedDate { get; set; }
		public DateTime? CapturedDate { get; set; }
		public DateTime? VoidedDate { get; set; }

        [StringLength(64)]
        public string TaxType { get; set; }

        [Column(TypeName = "Money")]
        public decimal Price { get; set; }
        [Column(TypeName = "Money")]
        public decimal PriceWithTax { get; set; }

        [Column(TypeName = "Money")]
        public decimal DiscountAmount { get; set; }
        [Column(TypeName = "Money")]
        public decimal DiscountAmountWithTax { get; set; }

        [Column(TypeName = "Money")]
        public decimal Total { get; set; }
        [Column(TypeName = "Money")]
        public decimal TotalWithTax { get; set; }

        [Column(TypeName = "Money")]
        public decimal TaxTotal { get; set; }
        public decimal TaxPercentRate { get; set; }

        public virtual ObservableCollection<AddressEntity> Addresses { get; set; }

		public string CustomerOrderId { get; set; }
		public virtual CustomerOrderEntity CustomerOrder { get; set; }

		public string ShipmentId { get; set; }
		public virtual ShipmentEntity Shipment { get; set; }

        public virtual ObservableCollection<DiscountEntity> Discounts { get; set; }
        public virtual ObservableCollection<TaxDetailEntity> TaxDetails { get; set; }


        public override OrderOperation ToModel(OrderOperation operation)
        {        
            var payment = operation as PaymentIn;
            if (payment == null)
                throw new NullReferenceException("payment");

            if (!this.Addresses.IsNullOrEmpty())
            {
                payment.BillingAddress =  this.Addresses.First().ToModel(AbstractTypeFactory<Address>.TryCreateInstance());
            }
            payment.TaxDetails = this.TaxDetails.Select(x => x.ToModel(AbstractTypeFactory<TaxDetail>.TryCreateInstance())).ToList();
            payment.Discounts = this.Discounts.Select(x => x.ToModel(AbstractTypeFactory<Discount>.TryCreateInstance())).ToList();

            base.ToModel(payment);

            payment.PaymentStatus = EnumUtility.SafeParse<PaymentStatus>(this.Status, PaymentStatus.Custom);

            return payment;
        }

        public override OperationEntity FromModel(OrderOperation operation, PrimaryKeyResolvingMap pkMap)
        {
            var payment = operation as PaymentIn;
            if (payment == null)
                throw new NullReferenceException("payment");

            base.FromModel(payment, pkMap);

            if(payment.PaymentMethod != null)
            {
                this.GatewayCode = payment.PaymentMethod != null ? payment.PaymentMethod.Code : payment.GatewayCode;
            }
            if (payment.BillingAddress != null)
            {
                this.Addresses = new ObservableCollection<AddressEntity>(new AddressEntity[] { AbstractTypeFactory<AddressEntity>.TryCreateInstance().FromModel(payment.BillingAddress) });
            }
            if (payment.TaxDetails != null)
            {
                this.TaxDetails = new ObservableCollection<TaxDetailEntity>(payment.TaxDetails.Select(x => AbstractTypeFactory<TaxDetailEntity>.TryCreateInstance().FromModel(x)));
            }
            if (payment.Discounts != null)
            {
                this.Discounts = new ObservableCollection<DiscountEntity>(payment.Discounts.Select(x => AbstractTypeFactory<DiscountEntity>.TryCreateInstance().FromModel(x)));
            }
            this.Status = payment.PaymentStatus.ToString();

            return this;
        }

        public override void Patch(OperationEntity operation)
        {
            base.Patch(operation);

            var target = operation as PaymentInEntity;
            if (target == null)
                throw new ArgumentNullException("target");

            target.Price = this.Price;
            target.PriceWithTax = this.PriceWithTax;
            target.DiscountAmount = this.DiscountAmount;
            target.DiscountAmountWithTax = this.DiscountAmountWithTax;
            target.TaxType = this.TaxType;
            target.TaxPercentRate = this.TaxPercentRate;
            target.TaxTotal = this.TaxTotal;
            target.Total = this.Total;
            target.TotalWithTax = this.TotalWithTax;

            target.CustomerId = this.CustomerId;
            target.OrganizationId = this.OrganizationId;
            target.GatewayCode = this.GatewayCode;
            target.Purpose = this.Purpose;
            target.OuterId = this.OuterId;
            target.Status = this.Status;
            target.AuthorizedDate = this.AuthorizedDate;
            target.CapturedDate = this.CapturedDate;
            target.VoidedDate = this.VoidedDate;
            target.IsCancelled = this.IsCancelled;
            target.CancelledDate = this.CancelledDate;
            target.CancelReason = this.CancelReason;
            target.Sum = this.Sum;
     
            if (!this.Addresses.IsNullCollection())
            {
                this.Addresses.Patch(target.Addresses, new AddressComparer(), (sourceAddress, targetAddress) => sourceAddress.Patch(targetAddress));
            }
            if (!this.TaxDetails.IsNullCollection())
            {
                var taxDetailComparer = AnonymousComparer.Create((TaxDetailEntity x) => x.Name);
                this.TaxDetails.Patch(target.TaxDetails, taxDetailComparer, (sourceTaxDetail, targetTaxDetail) => sourceTaxDetail.Patch(targetTaxDetail));
            }
            if (!this.Discounts.IsNullCollection())
            {
                var discountComparer = AnonymousComparer.Create((DiscountEntity x) => x.PromotionId);
                this.Discounts.Patch(target.Discounts, discountComparer, (sourceDiscount, targetDiscount) => sourceDiscount.Patch(targetDiscount));
            }
        }
    }
}
