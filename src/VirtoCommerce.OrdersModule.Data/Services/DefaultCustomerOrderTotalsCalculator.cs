using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.OrdersModule.Data.Services
{
    /// <summary>
    /// Respond for totals values calculation for Customer order and all nested objects
    /// </summary>
    public class DefaultCustomerOrderTotalsCalculator : ICustomerOrderTotalsCalculator
    {
        private readonly ICurrencyService _currencyService;

        public DefaultCustomerOrderTotalsCalculator(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }
        /// <summary>
        /// Order subtotal discount
        /// When a discount is applied to the cart subtotal, the tax calculation has already been applied, and is reflected in the tax subtotal.
        /// Therefore, a discount applying to the cart subtotal will occur after tax.
        /// For instance, if the cart subtotal is $100, and $15 is the tax subtotal, a cart - wide discount of 10 % will yield a total of $105($100 subtotal – $10 discount + $15 tax on the original $100).
        /// </summary>
        public virtual void CalculateTotals(CustomerOrder order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }
            //Calculate totals for line items
            if (!order.Items.IsNullOrEmpty())
            {
                foreach (var item in order.Items)
                {
                    CalculateLineItemTotals(item);
                }
            }
            //Calculate totals for shipments
            if (!order.Shipments.IsNullOrEmpty())
            {
                foreach (var shipment in order.Shipments)
                {
                    CalculateShipmentTotals(shipment);
                }
            }
            //Calculate totals for payments
            if (!order.InPayments.IsNullOrEmpty())
            {
                foreach (var payment in order.InPayments)
                {
                    CalculatePaymentTotals(payment);
                }
            }

            order.DiscountTotal = 0m;
            order.DiscountTotalWithTax = 0m;
            order.FeeTotal = order.Fee;
            order.TaxTotal = 0m;

            order.SubTotal = 0m;
            order.SubTotalWithTax = 0m;
            order.SubTotalTaxTotal = 0m;
            order.SubTotalDiscount = 0m;
            order.SubTotalDiscountWithTax = 0m;
            order.FeeTotalWithTax = 0m;

            if (order.Items != null)
            {
                order.SubTotal = order.Items.Sum(x => x.Price * x.Quantity);
                order.SubTotalWithTax = order.Items.Sum(x => x.PriceWithTax * x.Quantity);
                order.SubTotalTaxTotal += order.Items.Sum(x => x.TaxTotal);
                order.SubTotalDiscount = order.Items.Sum(x => x.DiscountTotal);
                order.SubTotalDiscountWithTax = order.Items.Sum(x => x.DiscountTotalWithTax);
                order.DiscountTotal += order.Items.Sum(x => x.DiscountTotal);
                order.DiscountTotalWithTax += order.Items.Sum(x => x.DiscountTotalWithTax);
                order.FeeTotal += order.Items.Sum(x => x.Fee);
                order.FeeTotalWithTax += order.Items.Sum(x => x.FeeWithTax);
                order.TaxTotal += order.Items.Sum(x => x.TaxTotal);
            }

            order.ShippingTotal = 0m;
            order.ShippingTotalWithTax = 0m;
            order.ShippingSubTotal = 0m;
            order.ShippingSubTotalWithTax = 0m;
            order.ShippingDiscountTotal = 0m;
            order.ShippingDiscountTotalWithTax = 0m;

            if (order.Shipments != null)
            {
                order.ShippingTotal = order.Shipments.Sum(x => x.Total);
                order.ShippingTotalWithTax = order.Shipments.Sum(x => x.TotalWithTax);
                order.ShippingSubTotal = order.Shipments.Sum(x => x.Price);
                order.ShippingSubTotalWithTax = order.Shipments.Sum(x => x.PriceWithTax);
                order.ShippingDiscountTotal = order.Shipments.Sum(x => x.DiscountAmount);
                order.ShippingDiscountTotalWithTax = order.Shipments.Sum(x => x.DiscountAmountWithTax);
                order.DiscountTotal += order.Shipments.Sum(x => x.DiscountAmount);
                order.DiscountTotalWithTax += order.Shipments.Sum(x => x.DiscountAmountWithTax);
                order.FeeTotal += order.Shipments.Sum(x => x.Fee);
                order.FeeTotalWithTax += order.Shipments.Sum(x => x.FeeWithTax);
                order.TaxTotal += order.Shipments.Sum(x => x.TaxTotal);
            }

            order.PaymentTotal = 0m;
            order.PaymentTotalWithTax = 0m;
            order.PaymentSubTotal = 0m;
            order.PaymentSubTotalWithTax = 0m;
            order.PaymentDiscountTotal = 0m;
            order.PaymentDiscountTotalWithTax = 0m;

            if (order.InPayments != null)
            {
                order.PaymentTotal = order.InPayments.Sum(x => x.Total);
                order.PaymentTotalWithTax = order.InPayments.Sum(x => x.TotalWithTax);
                order.PaymentSubTotal = order.InPayments.Sum(x => x.Price);
                order.PaymentSubTotalWithTax = order.InPayments.Sum(x => x.PriceWithTax);
                order.PaymentDiscountTotal = order.InPayments.Sum(x => x.DiscountAmount);
                order.PaymentDiscountTotalWithTax = order.InPayments.Sum(x => x.DiscountAmountWithTax);
                order.DiscountTotal += order.InPayments.Sum(x => x.DiscountAmount);
                order.DiscountTotalWithTax += order.InPayments.Sum(x => x.DiscountAmountWithTax);
                order.TaxTotal += order.InPayments.Sum(x => x.TaxTotal);
            }

            var taxFactor = 1 + order.TaxPercentRate;
            order.FeeWithTax = order.Fee * taxFactor;
            order.FeeTotalWithTax = order.FeeTotal * taxFactor;
            order.DiscountTotal += order.DiscountAmount;
            order.DiscountTotalWithTax += order.DiscountAmount * taxFactor;
            //Subtract from order tax total self discount tax amount
            order.TaxTotal -= order.DiscountAmount * order.TaxPercentRate;

            //Need to round all order totals
            var currency = GetCurrency(order.Currency).GetAwaiter().GetResult();
            order.SubTotal = currency.RoundingPolicy.RoundMoney(order.SubTotal, currency);
            order.SubTotalWithTax = currency.RoundingPolicy.RoundMoney(order.SubTotalWithTax, currency);
            order.SubTotalDiscount = currency.RoundingPolicy.RoundMoney(order.SubTotalDiscount, currency);
            order.SubTotalDiscountWithTax = currency.RoundingPolicy.RoundMoney(order.SubTotalDiscountWithTax, currency);
            order.TaxTotal = currency.RoundingPolicy.RoundMoney(order.TaxTotal, currency);
            order.DiscountTotal = currency.RoundingPolicy.RoundMoney(order.DiscountTotal, currency);
            order.DiscountTotalWithTax = currency.RoundingPolicy.RoundMoney(order.DiscountTotalWithTax, currency);
            order.Fee = currency.RoundingPolicy.RoundMoney(order.Fee, currency);
            order.FeeWithTax = currency.RoundingPolicy.RoundMoney(order.FeeWithTax, currency);
            order.FeeTotal = currency.RoundingPolicy.RoundMoney(order.FeeTotal, currency);
            order.FeeTotalWithTax = currency.RoundingPolicy.RoundMoney(order.FeeTotalWithTax, currency);
            order.ShippingTotal = currency.RoundingPolicy.RoundMoney(order.ShippingTotal, currency);
            order.ShippingTotalWithTax = currency.RoundingPolicy.RoundMoney(order.ShippingTotal, currency);
            order.ShippingSubTotal = currency.RoundingPolicy.RoundMoney(order.ShippingSubTotal, currency);
            order.ShippingSubTotalWithTax = currency.RoundingPolicy.RoundMoney(order.ShippingSubTotalWithTax, currency);
            order.PaymentTotal = currency.RoundingPolicy.RoundMoney(order.PaymentTotal, currency);
            order.PaymentTotalWithTax = currency.RoundingPolicy.RoundMoney(order.PaymentTotalWithTax, currency);
            order.PaymentSubTotal = currency.RoundingPolicy.RoundMoney(order.PaymentSubTotal, currency);
            order.PaymentSubTotalWithTax = currency.RoundingPolicy.RoundMoney(order.PaymentSubTotalWithTax, currency);
            order.PaymentDiscountTotal = currency.RoundingPolicy.RoundMoney(order.PaymentDiscountTotal, currency);
            order.PaymentDiscountTotalWithTax = currency.RoundingPolicy.RoundMoney(order.PaymentDiscountTotalWithTax, currency);

            order.Total = order.SubTotal + order.ShippingSubTotal + order.TaxTotal + order.PaymentSubTotal - order.DiscountTotal;
            order.Sum = order.Total;
        }

        protected virtual async Task<Currency> GetCurrency(string orderCurrencyName)
        {
            var currencies = await _currencyService.GetAllCurrenciesAsync();
            return currencies.First(c => c.Code == orderCurrencyName);
        }

        protected virtual void CalculatePaymentTotals(PaymentIn payment)
        {
            if (payment == null)
            {
                throw new ArgumentNullException(nameof(payment));
            }
            var taxFactor = 1 + payment.TaxPercentRate;
            payment.Total = payment.Price - payment.DiscountAmount;
            payment.TotalWithTax = payment.Total * taxFactor;
            payment.PriceWithTax = payment.Price * taxFactor;
            payment.DiscountAmountWithTax = payment.DiscountAmount * taxFactor;
            payment.TaxTotal = payment.Total * payment.TaxPercentRate;
            //For backward compatibility store in sum only payment amount instead of self total
            //payment.Sum = payment.Total;
        }

        protected virtual void CalculateShipmentTotals(Shipment shipment)
        {
            if (shipment == null)
            {
                throw new ArgumentNullException(nameof(shipment));
            }
            var taxFactor = 1 + shipment.TaxPercentRate;
            shipment.PriceWithTax = shipment.Price * taxFactor;
            shipment.DiscountAmountWithTax = shipment.DiscountAmount * taxFactor;
            shipment.FeeWithTax = shipment.Fee * taxFactor;
            shipment.Total = shipment.Price - shipment.DiscountAmount;
            shipment.TotalWithTax = shipment.PriceWithTax  - shipment.DiscountAmountWithTax;
            shipment.TaxTotal = shipment.Total * shipment.TaxPercentRate;
            shipment.Sum = shipment.Total;
        }

        protected virtual void CalculateLineItemTotals(LineItem lineItem)
        {
            if (lineItem == null)
            {
                throw new ArgumentNullException(nameof(lineItem));
            }
            var taxFactor = 1 + lineItem.TaxPercentRate;
            lineItem.PriceWithTax = lineItem.Price * taxFactor;
            lineItem.PlacedPrice = lineItem.Price - lineItem.DiscountAmount;
            lineItem.ExtendedPrice = lineItem.PlacedPrice * lineItem.Quantity;
            lineItem.DiscountAmountWithTax = lineItem.DiscountAmount * taxFactor;
            lineItem.DiscountTotal = lineItem.DiscountAmount * Math.Max(1, lineItem.Quantity);
            lineItem.FeeWithTax = lineItem.Fee * taxFactor;
            lineItem.PlacedPriceWithTax = lineItem.PlacedPrice * taxFactor;
            lineItem.ExtendedPriceWithTax = lineItem.PlacedPriceWithTax * lineItem.Quantity;
            lineItem.DiscountTotalWithTax = lineItem.DiscountAmountWithTax * Math.Max(1, lineItem.Quantity);
            lineItem.TaxTotal = lineItem.ExtendedPrice * lineItem.TaxPercentRate;
        }
    }
}
