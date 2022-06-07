using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Tax;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class CustomerOrder : OrderOperation, IHasTaxDetalization, ISupportSecurityScopes, ITaxable, IHasLanguage, IHasDiscounts, ICloneable
    {
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        [Obsolete("Use StoreId instead")]
        public string ChannelId { get; set; }

        public string StoreId { get; set; }

        public string StoreName { get; set; }

        public string OrganizationId { get; set; }

        public string OrganizationName { get; set; }

        [Obsolete("Use CustomerId instead")]
        public string EmployeeId { get; set; }

        [Obsolete("Use CustomerName instead")]
        public string EmployeeName { get; set; }

        /// <summary>
        /// The basis shopping cart id of which the order was created
        /// </summary>
        public string ShoppingCartId { get; set; }

        /// <summary>
        /// Flag determines that the order is the prototype
        /// </summary>
        public bool IsPrototype { get; set; }
        /// <summary>
        /// Internal number of order provided by customer
        /// </summary>
        public string PurchaseOrderNumber { get; set; }
        /// <summary>
        /// Number for subscription  associated with this order
        /// </summary>
        public string SubscriptionNumber { get; set; }
        /// <summary>
        /// Identifier for subscription  associated with this order
        /// </summary>
        public string SubscriptionId { get; set; }

        public override string ObjectType { get; set; } = typeof(CustomerOrder).FullName;

        public ICollection<Address> Addresses { get; set; }

        public ICollection<PaymentIn> InPayments { get; set; }

        public ICollection<LineItem> Items { get; set; }

        public ICollection<Shipment> Shipments { get; set; }
        
        #region IHasDiscounts
        public ICollection<Discount> Discounts { get; set; }
        #endregion

        /// <summary>
        /// When a discount is applied to the order, the tax calculation has already been applied, and is reflected in the tax.
        /// Therefore, a discount applying to the order  will occur after tax. 
        /// For instance, if the cart subtotal is $100, and $15 is the tax subtotal, a cart-wide discount of 10% will yield a total of $105 ($100 subtotal – $10 discount + $15 tax on the original $100).
        /// </summary>
        [Auditable]
        public decimal DiscountAmount { get; set; }

        #region ITaxDetailSupport Members

        public ICollection<TaxDetail> TaxDetails { get; set; }

        #endregion

        #region ISupportSecurityScopes Members
        public IEnumerable<string> Scopes { get; set; }
        #endregion
        
        /// <summary>
        /// Grand order total
        /// </summary>
        [Auditable]
        public virtual decimal Total { get; set; }
        
        /// <summary>
        /// Sum of the items prices
        /// </summary>
        public virtual decimal SubTotal { get; set; }

        /// <summary>
        /// Sum of the items prices with tax
        /// </summary>
        public virtual decimal SubTotalWithTax { get; set; }

        /// <summary>
        /// Sum of the items discount total
        /// </summary>
        public virtual decimal SubTotalDiscount { get; set; }

        /// <summary>
        /// Sum of the items discount total with tax
        /// </summary>
        public virtual decimal SubTotalDiscountWithTax { get; set; }

        /// <summary>
        /// Sum of the items tax total
        /// </summary>
        public virtual decimal SubTotalTaxTotal { get; set; }

        /// <summary>
        /// Sum of the shipments total
        /// </summary>
        public virtual decimal ShippingTotal { get; set; }

        /// <summary>
        /// Sum of the shipments total with tax
        /// </summary>
        public virtual decimal ShippingTotalWithTax { get; set; }

        /// <summary>
        /// Sum of the shipments prices
        /// </summary>
        public virtual decimal ShippingSubTotal { get; set; }

        /// <summary>
        /// Sum of hte shipments prices with tax
        /// </summary>
        public virtual decimal ShippingSubTotalWithTax { get; set; }

        /// <summary>
        /// Sum of the shipments discount amounts
        /// </summary>
        public virtual decimal ShippingDiscountTotal { get; set; }

        /// <summary>
        /// Sum of the shipments discount amounts with tax
        /// </summary>
        public virtual decimal ShippingDiscountTotalWithTax { get; set; }

        /// <summary>
        /// Reserved for future needs
        /// </summary>
        public virtual decimal ShippingTaxTotal { get; set; }

        /// <summary>
        /// Sum of the payments totals
        /// </summary>
        public virtual decimal PaymentTotal { get; set; }

        /// <summary>
        /// Sum of the payments totals with tax
        /// </summary>
        public virtual decimal PaymentTotalWithTax { get; set; }

        /// <summary>
        /// Sum of the payments prices
        /// </summary>
        public virtual decimal PaymentSubTotal { get; set; }

        /// <summary>
        /// Sum of the payments prices with tax
        /// </summary>
        public virtual decimal PaymentSubTotalWithTax { get; set; }

        /// <summary>
        /// Sum of the payments discount amounts
        /// </summary>
        public virtual decimal PaymentDiscountTotal { get; set; }

        /// <summary>
        /// Sum of the payments discount amounts with tax
        /// </summary>
        public virtual decimal PaymentDiscountTotalWithTax { get; set; }

        /// <summary>
        /// Reserved for future needs
        /// </summary>
        public virtual decimal PaymentTaxTotal { get; set; }

        /// <summary>
        /// Sum of the discount amounts of items, shipments and payments, and the order discount amount
        /// </summary>
        public virtual decimal DiscountTotal { get; set; }

        /// <summary>
        /// Sum of the discount amounts with tax of items, shipments and payments, and the order discount amount with tax
        /// </summary>
        public virtual decimal DiscountTotalWithTax { get; set; }
        
        /// <summary>
        /// Any extra fees applied to the order. This value comes from a cart
        /// </summary>
        [Auditable]
        public decimal Fee { get; set; }

        /// <summary>
        /// Order fee with applied tax factor
        /// </summary>
        public virtual decimal FeeWithTax { get; set; }

        /// <summary>
        /// Sum of the order fee and fees of the items, shipments and payments
        /// </summary>
        public virtual decimal FeeTotal { get; set; }

        /// <summary>
        /// FeeTotal with applied tax factor
        /// </summary>
        public virtual decimal FeeTotalWithTax { get; set; }

        /// <summary>
        /// Reserved for future needs
        /// </summary>
        public virtual decimal HandlingTotal { get; set; }

        /// <summary>
        /// Reserved for future needs
        /// </summary>
        public virtual decimal HandlingTotalWithTax { get; set; }

        #region ITaxable Members

        /// <summary>
        /// Tax category or type
        /// </summary>
        [Auditable]
        public string TaxType { get; set; }

        /// <summary>
        /// Sum of tax totals of the items, shipments and payments without the order discount amount with tax factor applied
        /// </summary>
        [Auditable]
        public virtual decimal TaxTotal { get; set; }

        [Auditable]
        public decimal TaxPercentRate { get; set; }

        #endregion

        #region ILanguageSupport Members

        public string LanguageCode { get; set; }

        #endregion

        public virtual void ReduceDetails(string responseGroup)
        {
            //Reduce details according to response group
            var orderResponseGroup = EnumUtility.SafeParseFlags(responseGroup, CustomerOrderResponseGroup.Full);

            if (!orderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithItems))
            {
                Items = null;
            }
            if (!orderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithShipments))
            {
                Shipments = null;
            }
            if (!orderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithInPayments))
            {
                InPayments = null;
            }
            if (!orderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithAddresses))
            {
                Addresses = null;
            }
            if (!orderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithDiscounts))
            {
                Discounts = null;
            }

            if (!orderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithPrices))
            {
                TaxPercentRate = 0m;
                ShippingTotalWithTax = 0m;
                PaymentTotalWithTax = 0m;
                DiscountAmount = 0m;
                Total = 0m;
                SubTotal = 0m;
                SubTotalWithTax = 0m;
                ShippingTotal = 0m;
                PaymentTotal = 0m;
                DiscountTotal = 0m;
                DiscountTotalWithTax = 0m;
                TaxTotal = 0m;
                Sum = 0m;
                Fee = 0m;
                FeeTotalWithTax = 0m;
                FeeTotal = 0m;
                FeeWithTax = 0m;
                HandlingTotal = 0m;
                HandlingTotalWithTax = 0m;
            }

            foreach (var shipment in Shipments ?? Array.Empty<Shipment>())
            {
                shipment.ReduceDetails(responseGroup);
            }
            foreach (var inPayment in InPayments ?? Array.Empty<PaymentIn>())
            {
                inPayment.ReduceDetails(responseGroup);
            }
            foreach (var item in Items ?? Array.Empty<LineItem>())
            {
                item.ReduceDetails(responseGroup);
            }

        }

        #region ICloneable members
        
        public override object Clone()
        {
            var result = base.Clone() as CustomerOrder;

                result.TaxDetails = TaxDetails?.Select(x => x.Clone()).OfType<TaxDetail>().ToList();
                result.Addresses = Addresses?.Select(x => x.Clone()).OfType<Address>().ToList();
                result.InPayments = InPayments?.Select(x => x.Clone()).OfType<PaymentIn>().ToList();
                result.Items = Items?.Select(x => x.Clone()).OfType<LineItem>().ToList();
                result.Shipments = Shipments?.Select(x => x.Clone()).OfType<Shipment>().ToList();
                result.Discounts = Discounts?.Select(x => x.Clone()).OfType<Discount>().ToList();

            return result;
        }

        #endregion
    }
}
