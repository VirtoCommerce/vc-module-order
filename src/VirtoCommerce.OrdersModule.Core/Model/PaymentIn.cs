using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Tax;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class PaymentIn : OrderOperation, IHasTaxDetalization, ITaxable, IHasDiscounts, ICloneable, IHasFeesDetalization
    {
        public string OrderId { get; set; }

        [Auditable]
        public string Purpose { get; set; }
        /// <summary>
        /// Payment method (gateway) code
        /// </summary>
        [Auditable]
        public string GatewayCode { get; set; }
        /// <summary>
        /// Payment method contains additional payment method information
        /// </summary>
        public PaymentMethod PaymentMethod { get; set; }
        public string OrganizationId { get; set; }
        public string OrganizationName { get; set; }

        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        [Auditable]
        public DateTime? IncomingDate { get; set; }
        public Address BillingAddress { get; set; }

        [Auditable]
        public PaymentStatus PaymentStatus { get; set; }
        [Auditable]
        public DateTime? AuthorizedDate { get; set; }
        [Auditable]
        public DateTime? CapturedDate { get; set; }
        [Auditable]
        public DateTime? VoidedDate { get; set; }

        public ProcessPaymentRequestResult ProcessPaymentResult { get; set; }

        //the self cost of the payment method
        public virtual decimal Price { get; set; }
        public virtual decimal PriceWithTax { get; set; }
        [Auditable]
        public virtual decimal Total { get; set; }

        public virtual decimal TotalWithTax { get; set; }
        [Auditable]
        public virtual decimal DiscountAmount { get; set; }
        public virtual decimal DiscountAmountWithTax { get; set; }
        public override string ObjectType { get; set; } = typeof(PaymentIn).FullName;
        public ICollection<FeeDetail> FeeDetails { get; set; }

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

        #region IHasDiscounts
        public ICollection<Discount> Discounts { get; set; }
        #endregion

        public ICollection<PaymentGatewayTransaction> Transactions { get; set; }

        public ICollection<Refund> Refunds { get; set; }
        public ICollection<Capture> Captures { get; set; }

        public virtual void ReduceDetails(string responseGroup)
        {
            //Reduce details according to response group
            var orderResponseGroup = EnumUtility.SafeParseFlags(responseGroup, CustomerOrderResponseGroup.Full);
            if (!orderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithAddresses))
            {
                BillingAddress = null;
            }
            if (!orderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithDiscounts))
            {
                Discounts = null;
            }
            if (!orderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithRefunds))
            {
                Refunds = null;
            }
            if (!orderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithCaptures))
            {
                Captures = null;
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
            var result = base.Clone() as PaymentIn;

            result.PaymentMethod = PaymentMethod?.Clone() as PaymentMethod;
            result.BillingAddress = BillingAddress?.Clone() as Address;
            result.ProcessPaymentResult = ProcessPaymentResult?.Clone() as ProcessPaymentRequestResult;
            result.Transactions = Transactions?.Select(x => x.Clone()).OfType<PaymentGatewayTransaction>().ToList();
            result.Discounts = Discounts?.Select(x => x.Clone()).OfType<Discount>().ToList();
            result.TaxDetails = TaxDetails?.Select(x => x.Clone()).OfType<TaxDetail>().ToList();
            result.FeeDetails = FeeDetails?.Select(x => x.Clone()).OfType<FeeDetail>().ToList();

            result.Refunds = Refunds?.Select(x => x.Clone()).OfType<Refund>().ToList();
            result.Captures = Captures?.Select(x => x.Clone()).OfType<Capture>().ToList();

            return result;
        }

        #endregion
    }
}
