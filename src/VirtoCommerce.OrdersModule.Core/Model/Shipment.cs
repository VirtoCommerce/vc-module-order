using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Tax;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Swagger;
using VirtoCommerce.ShippingModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    [SwaggerSchemaId("OrderShipment")]
    public class Shipment : OrderOperation, IHasTaxDetalization, ISupportCancellation, ITaxable, IHasDiscounts, ICloneable, IHasFeesDetalization
    {
        public string OrganizationId { get; set; }
        public string OrganizationName { get; set; }

        public string FulfillmentCenterId { get; set; }
        public string FulfillmentCenterName { get; set; }

        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }

        /// <summary>
        /// Current shipment method code 
        /// </summary>
        public string ShipmentMethodCode { get; set; }

        /// <summary>
        /// Current shipment option code 
        /// </summary>
        public string ShipmentMethodOption { get; set; }

        /// <summary>
        ///  Shipment method contains additional shipment method information
        /// </summary>
        public ShippingMethod ShippingMethod { get; set; }

        public string CustomerOrderId { get; set; }
        public CustomerOrder CustomerOrder { get; set; }

        public ICollection<ShipmentItem> Items { get; set; }

        public ICollection<ShipmentPackage> Packages { get; set; }

        public ICollection<PaymentIn> InPayments { get; set; }
        public ICollection<FeeDetail> FeeDetails { get; set; }

        [Auditable]
        public string WeightUnit { get; set; }
        [Auditable]
        public decimal? Weight { get; set; }
        [Auditable]
        public string MeasureUnit { get; set; }
        [Auditable]
        public decimal? Height { get; set; }
        [Auditable]
        public decimal? Length { get; set; }
        [Auditable]
        public decimal? Width { get; set; }

        #region IHasDiscounts
        public ICollection<Discount> Discounts { get; set; }
        #endregion

        public Address DeliveryAddress { get; set; }

        public virtual decimal Price { get; set; }
        public virtual decimal PriceWithTax { get; set; }
        [Auditable]
        public virtual decimal Total { get; set; }

        public virtual decimal TotalWithTax { get; set; }
        [Auditable]
        public virtual decimal DiscountAmount { get; set; }
        public virtual decimal DiscountAmountWithTax { get; set; }

        public string PickupLocationId { get; set; }

        //Any extra Fee
        [Auditable]
        public virtual decimal Fee { get; set; }
        public virtual decimal FeeWithTax { get; set; }

        /// <summary>
        /// Tracking information
        /// </summary>
        public string TrackingNumber { get; set; }
        public string TrackingUrl { get; set; }
        public DateTime? DeliveryDate { get; set; }

        public override string ObjectType { get; set; } = typeof(Shipment).FullName;

        public string VendorId { get; set; }

        #region ITaxable Members

        /// <summary>
        /// Tax category or type
        /// </summary>
        [Auditable]
        public string TaxType { get; set; }
        [Auditable]
        public decimal TaxTotal { get; set; }
        [Auditable]
        public decimal TaxPercentRate { get; set; }

        #endregion

        #region ITaxDetailSupport Members

        public ICollection<TaxDetail> TaxDetails { get; set; }

        #endregion

        public virtual void ReduceDetails(string responseGroup)
        {
            //Reduce details according to response group
            var orderResponseGroup = EnumUtility.SafeParseFlags(responseGroup, CustomerOrderResponseGroup.Full);
            if (!orderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithAddresses))
            {
                DeliveryAddress = null;
            }
            if (!orderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithDiscounts))
            {
                Discounts = null;
            }

            if (!orderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithPrices))
            {
                Price = 0m;
                PriceWithTax = 0m;
                DiscountAmount = 0m;
                DiscountAmountWithTax = 0m;
                Total = 0m;
                TotalWithTax = 0m;
                TaxTotal = 0m;
                TaxPercentRate = 0m;
                Sum = 0m;
            }

        }

        #region ICloneable members

        public override object Clone()
        {
            var result = base.Clone() as Shipment;

            result.DeliveryAddress = DeliveryAddress?.Clone() as Address;
            result.ShippingMethod = ShippingMethod?.Clone() as ShippingMethod;
            result.CustomerOrder = CustomerOrder?.Clone() as CustomerOrder;

            result.Items = Items?.Select(x => x.Clone()).OfType<ShipmentItem>().ToList();
            result.Packages = Packages?.Select(x => x.Clone()).OfType<ShipmentPackage>().ToList();
            result.InPayments = InPayments?.Select(x => x.Clone()).OfType<PaymentIn>().ToList();
            result.Discounts = Discounts?.Select(x => x.Clone()).OfType<Discount>().ToList();
            result.TaxDetails = TaxDetails?.Select(x => x.Clone()).OfType<TaxDetail>().ToList();
            result.FeeDetails = FeeDetails?.Select(x => x.Clone()).OfType<FeeDetail>().ToList();

            return result;
        }

        #endregion
    }
}
