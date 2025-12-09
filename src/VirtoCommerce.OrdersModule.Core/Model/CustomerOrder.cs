using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Tax;
using VirtoCommerce.OrdersModule.Core.Extensions;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class CustomerOrder : OrderOperation, IHasTaxDetalization, ISupportSecurityScopes, ITaxable, IHasLanguageCode, IHasDiscounts, IHasFeesDetalization, IHasRelevanceScore
    {
        public byte[] RowVersion { get; set; }

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string ChannelId { get; set; }

        public string StoreId { get; set; }

        public string StoreName { get; set; }

        public string OrganizationId { get; set; }

        public string OrganizationName { get; set; }

        public string EmployeeId { get; set; }

        public string EmployeeName { get; set; }

        /// <summary>
        /// The base shopping cart ID the order was created with
        /// </summary>
        public string ShoppingCartId { get; set; }

        /// <summary>
        /// This checkbox determines whether the order is a prototype
        /// </summary>
        public bool IsPrototype { get; set; }
        /// <summary>
        /// The order internal number provided by customer
        /// </summary>
        public string PurchaseOrderNumber { get; set; }
        /// <summary>
        /// Number of subscription associated with this order
        /// </summary>
        public string SubscriptionNumber { get; set; }
        /// <summary>
        /// The ID of subscription associated with this order
        /// </summary>
        public string SubscriptionId { get; set; }

        public override string ObjectType { get; set; } = typeof(CustomerOrder).FullName;

        public ICollection<Address> Addresses { get; set; }

        public ICollection<PaymentIn> InPayments { get; set; }

        public ICollection<LineItem> Items { get; set; }

        public ICollection<Shipment> Shipments { get; set; }

        public ICollection<FeeDetail> FeeDetails { get; set; }

        public double? RelevanceScore { get; set; }

        #region IHasDiscounts
        public ICollection<Discount> Discounts { get; set; }
        #endregion

        /// <summary>
        /// When a discount is applied to the order, the tax calculation has already been applied and is shown in the tax field.
        /// Therefore, the discount will not be taking tax into account. 
        /// For instance, if the cart subtotal is $100, and the tax subtotal is $15, a 10% discount will yield a total of $105 ($100 subtotal – $10 discount + $15 tax).
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
        /// Order grand total
        /// </summary>
        [Auditable]
        public virtual decimal Total { get; set; }

        /// <summary>
        /// Amount of the item prices
        /// </summary>
        public virtual decimal SubTotal { get; set; }

        /// <summary>
        /// Amount of the item prices with tax
        /// </summary>
        public virtual decimal SubTotalWithTax { get; set; }

        /// <summary>
        /// Amount of the item discount total
        /// </summary>
        public virtual decimal SubTotalDiscount { get; set; }

        /// <summary>
        /// Amount of the item discount total with tax
        /// </summary>
        public virtual decimal SubTotalDiscountWithTax { get; set; }

        /// <summary>
        /// Amount of the item tax total
        /// </summary>
        public virtual decimal SubTotalTaxTotal { get; set; }

        /// <summary>
        /// Amount of the shipment total
        /// </summary>
        public virtual decimal ShippingTotal { get; set; }

        /// <summary>
        /// Amount of the shipment total with tax
        /// </summary>
        public virtual decimal ShippingTotalWithTax { get; set; }

        /// <summary>
        /// Amount of the shipment prices
        /// </summary>
        public virtual decimal ShippingSubTotal { get; set; }

        /// <summary>
        /// Amount of the shipment prices with tax
        /// </summary>
        public virtual decimal ShippingSubTotalWithTax { get; set; }

        /// <summary>
        /// Amount of the shipment discount amounts
        /// </summary>
        public virtual decimal ShippingDiscountTotal { get; set; }

        /// <summary>
        /// Amount of the shipment discount amounts with tax
        /// </summary>
        public virtual decimal ShippingDiscountTotalWithTax { get; set; }

        /// <summary>
        /// Reserved for future needs
        /// </summary>
        public virtual decimal ShippingTaxTotal { get; set; }

        /// <summary>
        /// Amount of the payments totals
        /// </summary>
        public virtual decimal PaymentTotal { get; set; }

        /// <summary>
        /// Amount of the payment totals with tax
        /// </summary>
        public virtual decimal PaymentTotalWithTax { get; set; }

        /// <summary>
        /// Amount of the payment prices
        /// </summary>
        public virtual decimal PaymentSubTotal { get; set; }

        /// <summary>
        /// Amount of the payment prices with tax
        /// </summary>
        public virtual decimal PaymentSubTotalWithTax { get; set; }

        /// <summary>
        /// Amount of the payments discount amounts
        /// </summary>
        public virtual decimal PaymentDiscountTotal { get; set; }

        /// <summary>
        /// Amount of the payment discount amounts with tax
        /// </summary>
        public virtual decimal PaymentDiscountTotalWithTax { get; set; }

        /// <summary>
        /// Reserved for future needs
        /// </summary>
        public virtual decimal PaymentTaxTotal { get; set; }

        /// <summary>
        /// Amount of the discount amounts of items, shipments and payments, and the order discount amount
        /// </summary>
        public virtual decimal DiscountTotal { get; set; }

        /// <summary>
        /// Amount of the discount amounts with tax of items, shipments and payments, and the order discount amount with tax
        /// </summary>
        public virtual decimal DiscountTotalWithTax { get; set; }

        /// <summary>
        /// Any extra fees applied to the order. This value comes from the cart
        /// </summary>
        [Auditable]
        public decimal Fee { get; set; }

        /// <summary>
        /// Order fee with applied tax factor
        /// </summary>
        public virtual decimal FeeWithTax { get; set; }

        /// <summary>
        /// Amount of the order fee, as well as any item, shipment, and payment fees
        /// </summary>
        public virtual decimal FeeTotal { get; set; }

        /// <summary>
        /// Total fee with applied tax factor
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

        public bool IsAnonymous { get; set; }

        #region ITaxable Members

        /// <summary>
        /// Tax category or type
        /// </summary>
        [Auditable]
        public string TaxType { get; set; }

        /// <summary>
        /// Amount of tax totals for items, shipments, and payments without the order discount amount with tax factor applied
        /// </summary>
        [Auditable]
        public virtual decimal TaxTotal { get; set; }

        [Auditable]
        public decimal TaxPercentRate { get; set; }

        #endregion

        #region ILanguageSupport Members

        public string LanguageCode { get; set; }

        #endregion

        public override void ReduceDetails(string responseGroup)
        {
            base.ReduceDetails(responseGroup);

            // Reduce details according to the response group
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
                DiscountAmount = 0m;
                DiscountTotal = 0m;
                DiscountTotalWithTax = 0m;
                Fee = 0m;
                FeeTotal = 0m;
                FeeTotalWithTax = 0m;
                FeeWithTax = 0m;
                HandlingTotal = 0m;
                HandlingTotalWithTax = 0m;
                PaymentDiscountTotal = 0m;
                PaymentDiscountTotalWithTax = 0m;
                PaymentSubTotal = 0m;
                PaymentSubTotalWithTax = 0m;
                PaymentTaxTotal = 0m;
                PaymentTotal = 0m;
                PaymentTotalWithTax = 0m;
                ShippingDiscountTotal = 0m;
                ShippingDiscountTotalWithTax = 0m;
                ShippingSubTotal = 0m;
                ShippingSubTotalWithTax = 0m;
                ShippingTaxTotal = 0m;
                ShippingTotal = 0m;
                ShippingTotalWithTax = 0m;
                SubTotal = 0m;
                SubTotalDiscount = 0m;
                SubTotalDiscountWithTax = 0m;
                SubTotalTaxTotal = 0m;
                SubTotalWithTax = 0m;
                TaxPercentRate = 0m;
                TaxTotal = 0m;
                Total = 0m;
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

        public override void RestoreDetails(OrderOperation operation)
        {
            base.RestoreDetails(operation);

            if (operation is not CustomerOrder order)
            {
                return;
            }

            DiscountAmount = order.DiscountAmount;
            DiscountTotal = order.DiscountTotal;
            DiscountTotalWithTax = order.DiscountTotalWithTax;
            Fee = order.Fee;
            FeeTotal = order.FeeTotal;
            FeeTotalWithTax = order.FeeTotalWithTax;
            FeeWithTax = order.FeeWithTax;
            HandlingTotal = order.HandlingTotal;
            HandlingTotalWithTax = order.HandlingTotalWithTax;
            PaymentDiscountTotal = order.PaymentDiscountTotal;
            PaymentDiscountTotalWithTax = order.PaymentDiscountTotalWithTax;
            PaymentSubTotal = order.PaymentSubTotal;
            PaymentSubTotalWithTax = order.PaymentSubTotalWithTax;
            PaymentTaxTotal = order.PaymentTaxTotal;
            PaymentTotal = order.PaymentTotal;
            PaymentTotalWithTax = order.PaymentTotalWithTax;
            ShippingDiscountTotal = order.ShippingDiscountTotal;
            ShippingDiscountTotalWithTax = order.ShippingDiscountTotalWithTax;
            ShippingSubTotal = order.ShippingSubTotal;
            ShippingSubTotalWithTax = order.ShippingSubTotalWithTax;
            ShippingTaxTotal = order.ShippingTaxTotal;
            ShippingTotal = order.ShippingTotal;
            ShippingTotalWithTax = order.ShippingTotalWithTax;
            SubTotal = order.SubTotal;
            SubTotalDiscount = order.SubTotalDiscount;
            SubTotalDiscountWithTax = order.SubTotalDiscountWithTax;
            SubTotalTaxTotal = order.SubTotalTaxTotal;
            SubTotalWithTax = order.SubTotalWithTax;
            TaxPercentRate = order.TaxPercentRate;
            TaxTotal = order.TaxTotal;
            Total = order.Total;

            foreach (var shipment in order.Shipments ?? Array.Empty<Shipment>())
            {
                var targetShipment = Shipments?.FirstOrDefault(x => x.Id == shipment.Id);
                targetShipment?.RestoreDetails(shipment);
            }

            foreach (var payment in order.InPayments ?? Array.Empty<PaymentIn>())
            {
                var targetPayment = InPayments?.FirstOrDefault(x => x.Id == payment.Id);
                targetPayment?.RestoreDetails(payment);
            }

            foreach (var item in order.Items ?? Array.Empty<LineItem>())
            {
                var targetItem = Items?.FirstOrDefault(x => x.Id == item.Id);
                targetItem?.RestoreDetails(item);
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
            result.FeeDetails = FeeDetails?.Select(x => x.Clone()).OfType<FeeDetail>().ToList();

            result.FillChildOperations();

            return result;
        }

        #endregion
    }
}
