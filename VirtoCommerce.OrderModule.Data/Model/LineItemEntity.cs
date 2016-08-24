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
    public class LineItemEntity : AuditableEntity
    {
        public LineItemEntity()
        {
            Discounts = new NullCollection<DiscountEntity>();
            TaxDetails = new NullCollection<TaxDetailEntity>();
        }

        [StringLength(128)]
        public string PriceId { get; set; }
        [Required]
        [StringLength(3)]
        public string Currency { get; set; }
        [Column(TypeName = "Money")]
        public decimal BasePrice { get; set; }
        [Column(TypeName = "Money")]
        public decimal Price { get; set; }
        [Column(TypeName = "Money")]
        public decimal DiscountAmount { get; set; }
        [Column(TypeName = "Money")]
        public decimal Tax { get; set; }
        public int Quantity { get; set; }
        [Required]
        [StringLength(64)]
        public string ProductId { get; set; }
        [Required]
        [StringLength(64)]
        public string CatalogId { get; set; }

        [StringLength(64)]
        public string CategoryId { get; set; }
        [Required]
        [StringLength(64)]
        public string Sku { get; set; }

        [StringLength(64)]
        public string ProductType { get; set; }
        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        [StringLength(2048)]
        public string Comment { get; set; }

        public bool IsReccuring { get; set; }

        [StringLength(1028)]
        public string ImageUrl { get; set; }
        public bool IsGift { get; set; }
        [StringLength(64)]
        public string ShippingMethodCode { get; set; }
        [StringLength(64)]
        public string FulfilmentLocationCode { get; set; }

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

        public bool IsCancelled { get; set; }
        public DateTime? CancelledDate { get; set; }
        [StringLength(2048)]
        public string CancelReason { get; set; }

        public virtual ObservableCollection<DiscountEntity> Discounts { get; set; }

        public virtual ObservableCollection<TaxDetailEntity> TaxDetails { get; set; }

        public virtual CustomerOrderEntity CustomerOrder { get; set; }
        public string CustomerOrderId { get; set; }

        public virtual LineItem ToModel(LineItem lineItem)
        {
            if (lineItem == null)
                throw new ArgumentNullException("lineItem");

            lineItem.InjectFrom(this);
            if (!this.Discounts.IsNullOrEmpty())
            {
                lineItem.Discount = this.Discounts.First().ToModel(AbstractTypeFactory<Discount>.TryCreateInstance());
            }
            if (!this.TaxDetails.IsNullOrEmpty())
            {
                lineItem.TaxDetails = this.TaxDetails.Select(x => x.ToModel(AbstractTypeFactory<TaxDetail>.TryCreateInstance())).ToList();
            }
            return lineItem;
        }

        public virtual LineItemEntity FromModel(LineItem lineItem, PrimaryKeyResolvingMap pkMap)
        {
            if (lineItem == null)
                throw new ArgumentNullException("lineItem");

            pkMap.AddPair(lineItem, this);

            this.InjectFrom(lineItem);

            if (lineItem.Discount != null)
            {
                this.Discounts = new ObservableCollection<DiscountEntity>(new DiscountEntity[] { AbstractTypeFactory<DiscountEntity>.TryCreateInstance().FromModel(lineItem.Discount) });
            }
            if (lineItem.TaxDetails != null)
            {
                this.TaxDetails = new ObservableCollection<TaxDetailEntity>();
                this.TaxDetails.AddRange(lineItem.TaxDetails.Select(x => AbstractTypeFactory<TaxDetailEntity>.TryCreateInstance().FromModel(x)));
            }
            return this;
        }

        public virtual void Patch(LineItemEntity target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            target.BasePrice = this.BasePrice;
            target.Price = this.Price;
            target.Quantity = this.Quantity;
            target.DiscountAmount = this.DiscountAmount;
            target.Tax = this.Tax;
            target.Weight = this.Weight;
            target.Height = this.Height;
            target.Width = this.Width;
            target.MeasureUnit = this.MeasureUnit;
            target.WeightUnit = this.WeightUnit;
            target.Length = this.Length;
            target.TaxType = this.TaxType;
            target.IsCancelled = this.IsCancelled;
            target.CancelledDate = this.CancelledDate;
            target.CancelReason = this.CancelReason;  

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
        }
    }
}
