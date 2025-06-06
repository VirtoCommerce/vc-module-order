using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Tax;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.OrdersModule.Data.Model
{
    public class LineItemEntity : AuditableEntity, IHasOuterId, IDataEntity<LineItemEntity, LineItem>
    {
        [StringLength(128)]
        public string PriceId { get; set; }
        [Required]
        [StringLength(3)]
        public string Currency { get; set; }
        [Column(TypeName = "Money")]
        public decimal Price { get; set; }
        [Column(TypeName = "Money")]
        public decimal PriceWithTax { get; set; }
        [Column(TypeName = "Money")]
        public decimal DiscountAmount { get; set; }
        public bool IsDiscountAmountRounded { get; set; }
        [Column(TypeName = "Money")]
        public decimal DiscountAmountWithTax { get; set; }
        [Column(TypeName = "Money")]
        public decimal Fee { get; set; }
        [Column(TypeName = "Money")]
        public decimal FeeWithTax { get; set; }

        [Column(TypeName = "Money")]
        public decimal TaxTotal { get; set; }
        public decimal TaxPercentRate { get; set; }
        public int Quantity { get; set; }
        [Required]
        [StringLength(128)]
        public string ProductId { get; set; }
        [Required]
        [StringLength(64)]
        public string CatalogId { get; set; }

        [StringLength(64)]
        public string CategoryId { get; set; }
        [Required]
        [StringLength(128)]
        public string Sku { get; set; }

        [StringLength(64)]
        public string ProductType { get; set; }
        [Required]
        [StringLength(1024)]
        public string Name { get; set; }

        [StringLength(128)]
        public string ProductOuterId { get; set; }

        [StringLength(2048)]
        public string Comment { get; set; }

        [StringLength(64)]
        public string Status { get; set; }

        public bool IsReccuring { get; set; }

        [StringLength(1028)]
        public string ImageUrl { get; set; }
        public bool IsGift { get; set; }
        [StringLength(64)]
        public string ShippingMethodCode { get; set; }
        [StringLength(64)]
        public string FulfillmentLocationCode { get; set; }

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

        [NotMapped]
        public LineItem ModelLineItem { get; set; }

        [StringLength(128)]
        public string OuterId { get; set; }

        [StringLength(64)]
        public string FulfillmentCenterId { get; set; }

        [StringLength(128)]
        public string FulfillmentCenterName { get; set; }

        [StringLength(128)]
        public string VendorId { get; set; }

        public bool IsConfigured { get; set; }

        #region NavigationProperties

        public string CustomerOrderId { get; set; }
        public virtual CustomerOrderEntity CustomerOrder { get; set; }

        public virtual ObservableCollection<DiscountEntity> Discounts { get; set; } = new NullCollection<DiscountEntity>();

        public virtual ObservableCollection<TaxDetailEntity> TaxDetails { get; set; } = new NullCollection<TaxDetailEntity>();

        public virtual ObservableCollection<FeeDetailEntity> FeeDetails { get; set; } = new NullCollection<FeeDetailEntity>();

        public virtual ObservableCollection<OrderDynamicPropertyObjectValueEntity> DynamicPropertyObjectValues { get; set; }
            = new NullCollection<OrderDynamicPropertyObjectValueEntity>();

        public virtual ObservableCollection<ShipmentItemEntity> ShipmentItems { get; set; } = new NullCollection<ShipmentItemEntity>();

        public virtual ObservableCollection<RefundItemEntity> RefundItems { get; set; } = new NullCollection<RefundItemEntity>();

        public virtual ObservableCollection<CaptureItemEntity> CaptureItems { get; set; } = new NullCollection<CaptureItemEntity>();

        public virtual ObservableCollection<ConfigurationItemEntity> ConfigurationItems { get; set; } = new NullCollection<ConfigurationItemEntity>();

        #endregion

        public virtual LineItem ToModel(LineItem lineItem)
        {
            ArgumentNullException.ThrowIfNull(lineItem);

            lineItem.Id = Id;
            lineItem.CreatedDate = CreatedDate;
            lineItem.CreatedBy = CreatedBy;
            lineItem.ModifiedDate = ModifiedDate;
            lineItem.ModifiedBy = ModifiedBy;
            lineItem.OuterId = OuterId;

            lineItem.PriceId = PriceId;
            lineItem.CatalogId = CatalogId;
            lineItem.CategoryId = CategoryId;
            lineItem.Currency = Currency;
            lineItem.ProductId = ProductId;
            lineItem.Sku = Sku;
            lineItem.ProductType = ProductType;
            lineItem.Name = Name;
            lineItem.ProductOuterId = ProductOuterId;
            lineItem.ImageUrl = ImageUrl;
            lineItem.ShippingMethodCode = ShippingMethodCode;
            lineItem.FulfillmentLocationCode = FulfillmentLocationCode;

            lineItem.Price = Price;
            lineItem.PriceWithTax = PriceWithTax;
            lineItem.DiscountAmount = DiscountAmount;
            lineItem.IsDiscountAmountRounded = IsDiscountAmountRounded;
            lineItem.DiscountAmountWithTax = DiscountAmountWithTax;
            lineItem.Quantity = Quantity;
            lineItem.TaxTotal = TaxTotal;
            lineItem.Fee = Fee;
            lineItem.FeeWithTax = FeeWithTax;
            lineItem.TaxPercentRate = TaxPercentRate;
            lineItem.Weight = Weight;
            lineItem.Height = Height;
            lineItem.Width = Width;
            lineItem.MeasureUnit = MeasureUnit;
            lineItem.WeightUnit = WeightUnit;
            lineItem.Length = Length;
            lineItem.TaxType = TaxType;
            lineItem.IsCancelled = IsCancelled;
            lineItem.CancelledDate = CancelledDate;
            lineItem.CancelReason = CancelReason;
            lineItem.Comment = Comment;
            lineItem.Status = Status;
            lineItem.IsGift = IsGift;
            lineItem.FulfillmentCenterId = FulfillmentCenterId;
            lineItem.FulfillmentCenterName = FulfillmentCenterName;
            lineItem.VendorId = VendorId;
            lineItem.IsConfigured = IsConfigured;

            lineItem.Discounts = Discounts.Select(x => x.ToModel(AbstractTypeFactory<Discount>.TryCreateInstance())).ToList();
            lineItem.TaxDetails = TaxDetails.Select(x => x.ToModel(AbstractTypeFactory<TaxDetail>.TryCreateInstance())).ToList();
            lineItem.FeeDetails = FeeDetails.Select(x => x.ToModel(AbstractTypeFactory<FeeDetail>.TryCreateInstance())).ToList();
            lineItem.ConfigurationItems = ConfigurationItems.Select(x => x.ToModel(AbstractTypeFactory<ConfigurationItem>.TryCreateInstance())).ToList();

            lineItem.DynamicProperties = DynamicPropertyObjectValues.GroupBy(g => g.PropertyId).Select(x =>
            {
                var property = AbstractTypeFactory<DynamicObjectProperty>.TryCreateInstance();
                property.Id = x.Key;
                property.Name = x.FirstOrDefault()?.PropertyName;
                property.Values = x.Select(v => v.ToModel(AbstractTypeFactory<DynamicPropertyObjectValue>.TryCreateInstance())).ToArray();
                return property;
            }).ToArray();

            return lineItem;
        }

        public virtual LineItemEntity FromModel(LineItem lineItem, PrimaryKeyResolvingMap pkMap)
        {
            ArgumentNullException.ThrowIfNull(lineItem);

            ModelLineItem = lineItem;
            pkMap.AddPair(lineItem, this);

            Id = lineItem.Id;
            CreatedDate = lineItem.CreatedDate;
            CreatedBy = lineItem.CreatedBy;
            ModifiedDate = lineItem.ModifiedDate;
            ModifiedBy = lineItem.ModifiedBy;
            OuterId = lineItem.OuterId;

            PriceId = lineItem.PriceId;
            CatalogId = lineItem.CatalogId;
            CategoryId = lineItem.CategoryId;
            Currency = lineItem.Currency;
            ProductId = lineItem.ProductId;
            Sku = lineItem.Sku;
            ProductType = lineItem.ProductType;
            ProductOuterId = lineItem.ProductOuterId;
            Name = lineItem.Name;
            ImageUrl = lineItem.ImageUrl;
            ShippingMethodCode = lineItem.ShippingMethodCode;
            FulfillmentLocationCode = lineItem.FulfillmentLocationCode;

            Price = lineItem.Price;
            PriceWithTax = lineItem.PriceWithTax;
            DiscountAmount = lineItem.DiscountAmount;
            IsDiscountAmountRounded = lineItem.IsDiscountAmountRounded;
            DiscountAmountWithTax = lineItem.DiscountAmountWithTax;
            Fee = lineItem.Fee;
            FeeWithTax = lineItem.FeeWithTax;
            Quantity = lineItem.Quantity;
            TaxTotal = lineItem.TaxTotal;
            TaxPercentRate = lineItem.TaxPercentRate;
            Weight = lineItem.Weight;
            Height = lineItem.Height;
            Width = lineItem.Width;
            MeasureUnit = lineItem.MeasureUnit;
            WeightUnit = lineItem.WeightUnit;
            Length = lineItem.Length;
            TaxType = lineItem.TaxType;
            IsCancelled = lineItem.IsCancelled;
            CancelledDate = lineItem.CancelledDate;
            CancelReason = lineItem.CancelReason;
            Comment = lineItem.Comment;
            Status = lineItem.Status;

            IsGift = lineItem.IsGift ?? false;

            FulfillmentCenterId = lineItem.FulfillmentCenterId;
            FulfillmentCenterName = lineItem.FulfillmentCenterName;
            VendorId = lineItem.VendorId;

            IsConfigured = lineItem.IsConfigured;

            if (lineItem.Discounts != null)
            {
                Discounts = new ObservableCollection<DiscountEntity>();
                Discounts.AddRange(lineItem.Discounts.Select(x => AbstractTypeFactory<DiscountEntity>.TryCreateInstance().FromModel(x)));
            }

            if (lineItem.TaxDetails != null)
            {
                TaxDetails = new ObservableCollection<TaxDetailEntity>();
                TaxDetails.AddRange(lineItem.TaxDetails.Select(x => AbstractTypeFactory<TaxDetailEntity>.TryCreateInstance().FromModel(x)));
            }

            if (lineItem.FeeDetails != null)
            {
                FeeDetails = new ObservableCollection<FeeDetailEntity>();
                FeeDetails.AddRange(lineItem.FeeDetails.Select(x => AbstractTypeFactory<FeeDetailEntity>.TryCreateInstance().FromModel(x)));
            }

            if (lineItem.ConfigurationItems != null)
            {
                ConfigurationItems = new ObservableCollection<ConfigurationItemEntity>(lineItem.ConfigurationItems.Select(x => AbstractTypeFactory<ConfigurationItemEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            if (lineItem.DynamicProperties != null)
            {
                DynamicPropertyObjectValues = new ObservableCollection<OrderDynamicPropertyObjectValueEntity>(lineItem.DynamicProperties.SelectMany(p => p.Values
                    .Select(v => AbstractTypeFactory<OrderDynamicPropertyObjectValueEntity>.TryCreateInstance().FromModel(v, lineItem, p))).OfType<OrderDynamicPropertyObjectValueEntity>());
            }

            return this;
        }

        public virtual void Patch(LineItemEntity target)
        {
            ArgumentNullException.ThrowIfNull(target);

            target.OuterId = OuterId;
            target.Quantity = Quantity;
            target.Weight = Weight;
            target.Height = Height;
            target.Width = Width;
            target.MeasureUnit = MeasureUnit;
            target.WeightUnit = WeightUnit;
            target.Length = Length;
            target.TaxType = TaxType;
            target.IsCancelled = IsCancelled;
            target.CancelledDate = CancelledDate;
            target.CancelReason = CancelReason;
            target.Comment = Comment;
            target.Status = Status;
            target.FulfillmentCenterId = FulfillmentCenterId;
            target.FulfillmentCenterName = FulfillmentCenterName;
            target.VendorId = VendorId;
            target.IsConfigured = IsConfigured;
            target.IsDiscountAmountRounded = IsDiscountAmountRounded;

            // Patch prices if there are non 0 prices in the patching entity, or all patched entity prices are 0
            var isNeedPatch = GetNonCalculatablePrices().Any(x => x != 0m) || target.GetNonCalculatablePrices().All(x => x == 0m);

            if (isNeedPatch)
            {
                target.TaxPercentRate = TaxPercentRate;
                target.Price = Price;
                target.DiscountAmount = DiscountAmount;
                target.PriceWithTax = PriceWithTax;
                target.DiscountAmountWithTax = DiscountAmountWithTax;
                target.TaxTotal = TaxTotal;
                target.Fee = Fee;
                target.FeeWithTax = FeeWithTax;
            }

            if (!Discounts.IsNullCollection())
            {
                var discountComparer = AnonymousComparer.Create((DiscountEntity x) => x.PromotionId);
                Discounts.Patch(target.Discounts, discountComparer, (sourceDiscount, targetDiscount) => sourceDiscount.Patch(targetDiscount));
            }

            if (!TaxDetails.IsNullCollection())
            {
                var taxDetailComparer = AnonymousComparer.Create((TaxDetailEntity x) => x.Name);
                TaxDetails.Patch(target.TaxDetails, taxDetailComparer, (sourceTaxDetail, targetTaxDetail) => sourceTaxDetail.Patch(targetTaxDetail));
            }

            if (!FeeDetails.IsNullCollection())
            {
                var feeDetailComparer = AnonymousComparer.Create((FeeDetailEntity x) => x.FeeId);
                FeeDetails.Patch(target.FeeDetails, feeDetailComparer, (sourceFeeDetail, targetFeeDetail) => sourceFeeDetail.Patch(targetFeeDetail));
            }

            if (!ConfigurationItems.IsNullCollection())
            {
                ConfigurationItems.Patch(target.ConfigurationItems, (sourceConfigurationItem, targetConfigurationItem) => sourceConfigurationItem.Patch(targetConfigurationItem));
            }

            if (!DynamicPropertyObjectValues.IsNullCollection())
            {
                DynamicPropertyObjectValues.Patch(target.DynamicPropertyObjectValues, (sourceDynamicPropertyObjectValues, targetDynamicPropertyObjectValues) => sourceDynamicPropertyObjectValues.Patch(targetDynamicPropertyObjectValues));
            }
        }

        public virtual void ResetPrices()
        {
            Price = 0m;
            PriceWithTax = 0m;
            DiscountAmount = 0m;
            DiscountAmountWithTax = 0m;
            TaxTotal = 0m;
            TaxPercentRate = 0m;
            Fee = 0m;
            FeeWithTax = 0m;
        }

        public virtual IEnumerable<decimal> GetNonCalculatablePrices()
        {
            yield return TaxPercentRate;
            yield return Price;
            yield return DiscountAmount;
            yield return Fee;
        }
    }
}
