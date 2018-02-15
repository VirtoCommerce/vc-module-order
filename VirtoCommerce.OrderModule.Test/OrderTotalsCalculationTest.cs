using System.Collections.Generic;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.OrderModule.Data.Services;
using Xunit;

namespace VirtoCommerce.OrderModule.Test
{
    public class OrderTotalsCalculationTest
    {
        [Fact]
        public void CalculateTotals_ShouldBe_RightTotals()
        {
            var item1 = new LineItem { Price = 10.99m, DiscountAmount = 1.33m, TaxPercentRate = 0.12m, Fee = 0.33m, Quantity = 2 };
            var item2 = new LineItem { Price = 55.22m, DiscountAmount = 5.89m, TaxPercentRate = 0.12m, Fee = 0.12m, Quantity = 5 };
            var item3 = new LineItem { Price = 88.45m, DiscountAmount = 10.78m, TaxPercentRate = 0.12m, Fee = 0.08m, Quantity = 12 };
            var payment = new PaymentIn { Price = 44.52m, DiscountAmount = 10, TaxPercentRate = 0.12m };
            var shipment = new Shipment { Price = 22.0m, DiscountAmount = 5m, TaxPercentRate = 0.12m };

            var order = new CustomerOrder
            {
                TaxPercentRate = 0.12m,
                Fee = 13.11m,
                Items = new List<LineItem> { item1, item2, item3 },
                InPayments = new List<PaymentIn> { payment },
                Shipments = new List<Shipment> { shipment }
            };
            var totalsCalculator = new DefaultCustomerOrderTotalsCalculator();
            totalsCalculator.CalculateTotals(order);

            Assert.Equal(item1.PriceWithTax, 12.3088m);
            Assert.Equal(item1.PlacedPrice, 9.66m);
            Assert.Equal(item1.ExtendedPrice, 19.32m);
            Assert.Equal(item1.DiscountAmountWithTax, 1.4896m);
            Assert.Equal(item1.DiscountTotal, 2.66m);
            Assert.Equal(item1.FeeWithTax, 0.3696m);
            Assert.Equal(item1.PlacedPriceWithTax, 10.8192m);
            Assert.Equal(item1.ExtendedPriceWithTax, 21.6384m);
            Assert.Equal(item1.DiscountTotalWithTax, 2.9792m);
            Assert.Equal(item1.TaxTotal, 2.358m);

            Assert.Equal(shipment.DiscountAmountWithTax, 5.6m);
            Assert.Equal(shipment.PriceWithTax, 24.64m);
            Assert.Equal(shipment.FeeWithTax, 0.0m);
            Assert.Equal(shipment.Total, 17.0m);
            Assert.Equal(shipment.TotalWithTax, 19.04m);
            Assert.Equal(shipment.TaxTotal, 2.04m);

            Assert.Equal(payment.Total, 34.52m);
            Assert.Equal(payment.PriceWithTax, 49.8624m);
            Assert.Equal(payment.TotalWithTax, 38.6624m);
            Assert.Equal(payment.DiscountAmountWithTax, 11.2m);
            Assert.Equal(payment.TaxTotal, 4.1424m);

            Assert.Equal(order.SubTotal, 1359.48m);
            Assert.Equal(order.SubTotalDiscount, 161.47m);
            Assert.Equal(order.SubTotalDiscountWithTax, 180.8464m);
            Assert.Equal(order.SubTotalTaxTotal, 143.8248m);
            Assert.Equal(order.SubTotalWithTax, 1522.6176m);
            Assert.Equal(order.ShippingSubTotal, 22.00m);
            Assert.Equal(order.ShippingSubTotalWithTax, 24.64m);
            Assert.Equal(order.PaymentSubTotal, 44.52m);
            Assert.Equal(order.PaymentSubTotalWithTax, 49.8624m);
            Assert.Equal(order.TaxTotal, 150.0072m);
            Assert.Equal(order.DiscountTotal, 176.47m);
            Assert.Equal(order.DiscountTotalWithTax, 197.6464m);
            Assert.Equal(order.FeeTotal, 13.64m);
            Assert.Equal(order.FeeTotalWithTax, 15.2768m);
            Assert.Equal(order.FeeWithTax, 14.6832m);
            Assert.Equal(order.Total, 1413.1772m);
        }
    }
}
