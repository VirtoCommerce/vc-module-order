using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

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

            var ordersByCurrency = new Dictionary<string, CustomerOrder>
            {
                { order.Currency, order }
            };

            var currencyCodes = (order.Items?.Select(x => x.Currency) ?? [])
                .Concat(order.Shipments?.Select(x => x.Currency) ?? [])
                .Concat(order.InPayments?.Select(x => x.Currency) ?? [])
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct();

            foreach (var currencyCode in currencyCodes)
            {
                AddCustomerOrderByCurrency(ordersByCurrency, currencyCode);
            }

            var allCurrencies = GetAllCurrencies();

            foreach (var (currencyCode, orderCart) in ordersByCurrency)
            {
                orderCart.DiscountTotal = 0m;
                orderCart.DiscountTotalWithTax = 0m;
                orderCart.FeeTotal = orderCart.Fee;
                orderCart.FeeTotalWithTax = 0m;
                orderCart.TaxTotal = 0m;

                // Line items
                var currencyItems = order.Items?.Where(x => x.Currency == currencyCode).ToList() ?? [];
                orderCart.SubTotal = currencyItems.Sum(x => x.ListTotal);
                orderCart.SubTotalWithTax = currencyItems.Sum(x => x.ListTotalWithTax);
                orderCart.SubTotalTaxTotal = currencyItems.Sum(x => x.TaxTotal);
                orderCart.SubTotalDiscount = currencyItems.Sum(x => x.DiscountTotal);
                orderCart.SubTotalDiscountWithTax = currencyItems.Sum(x => x.DiscountTotalWithTax);
                orderCart.DiscountTotal += currencyItems.Sum(x => x.DiscountTotal);
                orderCart.DiscountTotalWithTax += currencyItems.Sum(x => x.DiscountTotalWithTax);
                orderCart.FeeTotal += currencyItems.Sum(x => x.Fee);
                orderCart.FeeTotalWithTax += currencyItems.Sum(x => x.FeeWithTax);
                orderCart.TaxTotal += currencyItems.Sum(x => x.TaxTotal);

                // Shipments
                var currencyShipments = order.Shipments?.Where(x => x.Currency == currencyCode).ToList() ?? [];
                orderCart.ShippingTotal = currencyShipments.Sum(x => x.Total);
                orderCart.ShippingTotalWithTax = currencyShipments.Sum(x => x.TotalWithTax);
                orderCart.ShippingSubTotal = currencyShipments.Sum(x => x.Price);
                orderCart.ShippingSubTotalWithTax = currencyShipments.Sum(x => x.PriceWithTax);
                orderCart.ShippingDiscountTotal = currencyShipments.Sum(x => x.DiscountAmount);
                orderCart.ShippingDiscountTotalWithTax = currencyShipments.Sum(x => x.DiscountAmountWithTax);
                orderCart.DiscountTotal += currencyShipments.Sum(x => x.DiscountAmount);
                orderCart.DiscountTotalWithTax += currencyShipments.Sum(x => x.DiscountAmountWithTax);
                orderCart.FeeTotal += currencyShipments.Sum(x => x.Fee);
                orderCart.FeeTotalWithTax += currencyShipments.Sum(x => x.FeeWithTax);
                orderCart.TaxTotal += currencyShipments.Sum(x => x.TaxTotal);

                // Payments
                var currencyPayments = order.InPayments?.Where(x => x.Currency == currencyCode).ToList() ?? [];
                orderCart.PaymentTotal = currencyPayments.Sum(x => x.Total);
                orderCart.PaymentTotalWithTax = currencyPayments.Sum(x => x.TotalWithTax);
                orderCart.PaymentSubTotal = currencyPayments.Sum(x => x.Price);
                orderCart.PaymentSubTotalWithTax = currencyPayments.Sum(x => x.PriceWithTax);
                orderCart.PaymentDiscountTotal = currencyPayments.Sum(x => x.DiscountAmount);
                orderCart.PaymentDiscountTotalWithTax = currencyPayments.Sum(x => x.DiscountAmountWithTax);
                orderCart.DiscountTotal += currencyPayments.Sum(x => x.DiscountAmount);
                orderCart.DiscountTotalWithTax += currencyPayments.Sum(x => x.DiscountAmountWithTax);
                orderCart.TaxTotal += currencyPayments.Sum(x => x.TaxTotal);

                var taxFactor = 1 + orderCart.TaxPercentRate;
                orderCart.FeeWithTax = orderCart.Fee * taxFactor;
                orderCart.FeeTotalWithTax = orderCart.FeeTotal * taxFactor;
                orderCart.DiscountTotal += orderCart.DiscountAmount;
                orderCart.DiscountTotalWithTax += orderCart.DiscountAmount * taxFactor;
                //Subtract from order tax total self discount tax amount
                orderCart.TaxTotal -= orderCart.DiscountAmount * orderCart.TaxPercentRate;

                //Need to round all order totals
                var currency = GetCurrency(allCurrencies, orderCart.Currency);
                orderCart.SubTotal = currency.RoundingPolicy.RoundMoney(orderCart.SubTotal, currency);
                orderCart.SubTotalWithTax = currency.RoundingPolicy.RoundMoney(orderCart.SubTotalWithTax, currency);
                orderCart.SubTotalDiscount = currency.RoundingPolicy.RoundMoney(orderCart.SubTotalDiscount, currency);
                orderCart.SubTotalDiscountWithTax = currency.RoundingPolicy.RoundMoney(orderCart.SubTotalDiscountWithTax, currency);
                orderCart.TaxTotal = currency.RoundingPolicy.RoundMoney(orderCart.TaxTotal, currency);
                orderCart.DiscountTotal = currency.RoundingPolicy.RoundMoney(orderCart.DiscountTotal, currency);
                orderCart.DiscountTotalWithTax = currency.RoundingPolicy.RoundMoney(orderCart.DiscountTotalWithTax, currency);
                orderCart.Fee = currency.RoundingPolicy.RoundMoney(orderCart.Fee, currency);
                orderCart.FeeWithTax = currency.RoundingPolicy.RoundMoney(orderCart.FeeWithTax, currency);
                orderCart.FeeTotal = currency.RoundingPolicy.RoundMoney(orderCart.FeeTotal, currency);
                orderCart.FeeTotalWithTax = currency.RoundingPolicy.RoundMoney(orderCart.FeeTotalWithTax, currency);
                orderCart.ShippingTotal = currency.RoundingPolicy.RoundMoney(orderCart.ShippingTotal, currency);
                orderCart.ShippingTotalWithTax = currency.RoundingPolicy.RoundMoney(orderCart.ShippingTotalWithTax, currency);
                orderCart.ShippingSubTotal = currency.RoundingPolicy.RoundMoney(orderCart.ShippingSubTotal, currency);
                orderCart.ShippingSubTotalWithTax = currency.RoundingPolicy.RoundMoney(orderCart.ShippingSubTotalWithTax, currency);
                orderCart.PaymentTotal = currency.RoundingPolicy.RoundMoney(orderCart.PaymentTotal, currency);
                orderCart.PaymentTotalWithTax = currency.RoundingPolicy.RoundMoney(orderCart.PaymentTotalWithTax, currency);
                orderCart.PaymentSubTotal = currency.RoundingPolicy.RoundMoney(orderCart.PaymentSubTotal, currency);
                orderCart.PaymentSubTotalWithTax = currency.RoundingPolicy.RoundMoney(orderCart.PaymentSubTotalWithTax, currency);
                orderCart.PaymentDiscountTotal = currency.RoundingPolicy.RoundMoney(orderCart.PaymentDiscountTotal, currency);
                orderCart.PaymentDiscountTotalWithTax = currency.RoundingPolicy.RoundMoney(orderCart.PaymentDiscountTotalWithTax, currency);

                orderCart.Total = orderCart.SubTotal + orderCart.ShippingSubTotal + orderCart.TaxTotal + orderCart.PaymentSubTotal + orderCart.FeeTotal - orderCart.DiscountTotal;
                orderCart.Sum = orderCart.Total;
            }

            order.OrderTotals = ordersByCurrency.Select(x =>
            {
                var cartTotal = AbstractTypeFactory<OrderTotal>.TryCreateInstance();

                cartTotal.CurrencyCode = x.Value.Currency;
                cartTotal.Total = x.Value.Total;
                cartTotal.SubTotal = x.Value.SubTotal;
                cartTotal.TaxTotal = x.Value.TaxTotal;
                cartTotal.DiscountTotal = x.Value.DiscountTotal;

                return cartTotal;
            }).ToList();
        }

        protected virtual IList<Currency> GetAllCurrencies()
        {
            return _currencyService.GetAllCurrenciesAsync().GetAwaiter().GetResult().ToList();
        }

        protected virtual Currency GetCurrency(IList<Currency> currencies, string currencyCode)
        {
            return currencies.First(c => c.Code == currencyCode);
        }

        [Obsolete("No longer used. Use GetAllCurrencies() and GetCurrency(IList<Currency>, string) instead.", false, DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
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
            shipment.Total = shipment.Price + shipment.Fee - shipment.DiscountAmount;
            shipment.TotalWithTax = shipment.PriceWithTax + shipment.FeeWithTax - shipment.DiscountAmountWithTax;
            shipment.TaxTotal = shipment.Total * shipment.TaxPercentRate;
            shipment.Sum = shipment.Total;
        }

        protected virtual void CalculateLineItemTotals(LineItem lineItem)
        {
            ArgumentNullException.ThrowIfNull(lineItem);

            var quantity = Math.Max(1, lineItem.Quantity);
            var currency = _currencyService.GetAllCurrenciesAsync().GetAwaiter().GetResult().First(c => c.Code == lineItem.Currency);

            lineItem.ListTotal = lineItem.Price * quantity;
            lineItem.PlacedPrice = lineItem.Price - lineItem.DiscountAmount;
            lineItem.DiscountTotal = currency.RoundingPolicy.RoundMoney(lineItem.DiscountAmount * quantity, currency);
            lineItem.ExtendedPrice = lineItem.ListTotal - lineItem.DiscountTotal;

            var taxFactor = 1 + lineItem.TaxPercentRate;

            lineItem.PriceWithTax = lineItem.Price * taxFactor;
            lineItem.ListTotalWithTax = lineItem.ListTotal * taxFactor;
            lineItem.PlacedPriceWithTax = lineItem.PlacedPrice * taxFactor;
            lineItem.ExtendedPriceWithTax = lineItem.ExtendedPrice * taxFactor;
            lineItem.DiscountAmountWithTax = lineItem.DiscountAmount * taxFactor;
            lineItem.DiscountTotalWithTax = lineItem.DiscountTotal * taxFactor;
            lineItem.FeeWithTax = lineItem.Fee * taxFactor;

            lineItem.TaxTotal = lineItem.ExtendedPrice * lineItem.TaxPercentRate;
        }

        private static CustomerOrder AddCustomerOrderByCurrency(Dictionary<string, CustomerOrder> orderByCurrency, string currencyCode)
        {
            if (!orderByCurrency.TryGetValue(currencyCode, out var currencyOrder))
            {
                currencyOrder = AbstractTypeFactory<CustomerOrder>.TryCreateInstance();
                currencyOrder.Currency = currencyCode;
                orderByCurrency.Add(currencyCode, currencyOrder);
            }

            return currencyOrder;
        }
    }
}
