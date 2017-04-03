using System;
using System.Collections.Generic;
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
	public class DiscountEntity : Entity
	{
		[StringLength(64)]
		public string PromotionId { get; set; }
		[StringLength(1024)]
		public string PromotionDescription { get; set; }
		[Required]
		[StringLength(3)]
		public string Currency { get; set; }
		[Column(TypeName = "Money")]
		public decimal DiscountAmount { get; set; }
        [Column(TypeName = "Money")]
        public decimal DiscountAmountWithTax { get; set; }
        [StringLength(64)]
		public string CouponCode { get; set; }
		[StringLength(1024)]
		public string CouponInvalidDescription { get; set; }

		public virtual CustomerOrderEntity CustomerOrder { get; set; }
		public string CustomerOrderId { get; set; }

		public virtual ShipmentEntity Shipment { get; set; }
		public string ShipmentId { get; set; }

		public virtual LineItemEntity LineItem { get; set; }
		public string LineItemId { get; set; }

        public virtual PaymentInEntity PaymentIn { get; set; }
        public string PaymentInId { get; set; }

        public virtual Discount ToModel(Discount discount)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            discount.InjectFrom(this);
            discount.Coupon = this.CouponCode; 
      
            return discount;
        }

        public virtual DiscountEntity FromModel(Discount discount)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

        
            this.InjectFrom(discount);         
            this.CouponCode = discount.Coupon;

            return this;
        }

        public virtual void Patch(DiscountEntity target)
        {
            target.CouponCode = this.CouponCode;
            target.Currency = this.Currency;
            target.DiscountAmount = this.DiscountAmount;
            target.DiscountAmountWithTax = this.DiscountAmountWithTax;
            target.PromotionDescription = this.PromotionDescription;
            target.PromotionId = this.PromotionId;       
        }
    }
}
