using System;
using System.Collections.Generic;
using Moq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Services;
using Xunit;
using LineItem = VirtoCommerce.OrdersModule.Core.Model.LineItem;
using Shipment = VirtoCommerce.OrdersModule.Core.Model.Shipment;

namespace VirtoCommerce.OrdersModule.Tests
{
    [Trait("Category", "CI")]
    public class OrderTotalsCalculationTest
    {
        public static IEnumerable<object[]> Data =>
        [
            //                                                                    Expected  Expected       Expected
            // MidpointRounding,             ListPrice, DiscountAmount, Quantity, SubTotal, DiscountTotal, Total
            [MidpointRounding.AwayFromZero,  49.95m,      4.9950m,       1,         49.95m,    5.00m,        44.95m],
            [MidpointRounding.ToZero,        49.95m,      4.9950m,       1,         49.95m,    4.99m,        44.96m],
            [MidpointRounding.AwayFromZero,  26.25m,      1.3125m,       1,         26.25m,    1.31m,        24.94m],
            [MidpointRounding.ToZero,        26.25m,      1.3125m,       1,         26.25m,    1.31m,        24.94m],
            [MidpointRounding.AwayFromZero,  26.25m,      1.3125m,       3,         78.75m,    3.94m,        74.81m],
            [MidpointRounding.ToZero,        26.25m,      1.3125m,       3,         78.75m,    3.93m,        74.82m],
            [MidpointRounding.AwayFromZero, 422.50m,    190.1250m,       1,        422.50m,  190.13m,       232.37m],
            [MidpointRounding.ToZero,       422.50m,    190.1250m,       1,        422.50m,  190.12m,       232.38m],
            [MidpointRounding.AwayFromZero, 422.50m,    190.1250m,      10,       4225.00m, 1901.25m,      2323.75m],
            [MidpointRounding.ToZero,       422.50m,    190.1250m,      10,       4225.00m, 1901.25m,      2323.75m],
        ];

        [Theory]
        [MemberData(nameof(Data))]
        public void CalculateTotals_LineItemDiscountTotal_MustBeRounded(
            MidpointRounding midpointRounding,
            decimal listPrice,
            decimal discountAmount,
            int quantity,
            decimal expectedSubTotal,
            decimal expectedDiscountTotal,
            decimal expectedTotal)
        {
            // Arrange
            var lineItem = new LineItem
            {
                Price = listPrice,
                DiscountAmount = discountAmount,
                Quantity = quantity,
            };

            var order = new CustomerOrder
            {
                Items = [lineItem],
            };

            var currency = new Currency(new Language("en-US"), code: null)
            {
                MidpointRounding = midpointRounding.ToString(),
                RoundingPolicy = new DefaultMoneyRoundingPolicy()
            };

            var totalsCalculator = GetTotalsCalculator(currency);

            // Act
            totalsCalculator.CalculateTotals(order);

            // Assert
            Assert.Equal(expectedSubTotal, order.SubTotal);
            Assert.Equal(expectedDiscountTotal, order.DiscountTotal);
            Assert.Equal(expectedTotal, order.Total);

            Assert.Equal(expectedSubTotal, lineItem.ListTotal);
            Assert.Equal(expectedDiscountTotal, lineItem.DiscountTotal);
            Assert.Equal(expectedTotal, lineItem.ExtendedPrice);
        }

        private DefaultCustomerOrderTotalsCalculator GetTotalsCalculator(Currency currency)
        {
            var currencyServiceMock = new Mock<ICurrencyService>();
            currencyServiceMock.Setup(c => c.GetAllCurrenciesAsync()).ReturnsAsync(new List<Currency> { currency });
            return new DefaultCustomerOrderTotalsCalculator(currencyServiceMock.Object);
        }

        [Fact]
        public void CalculateTotals_ClearAllItems_TotalsMustBeZero()
        {
            var item1 = new LineItem { Price = 10.99m, DiscountAmount = 1.33m, TaxPercentRate = 0.12m, Fee = 0.33m, Quantity = 2 };
            var item2 = new LineItem { Price = 55.22m, DiscountAmount = 5.89m, TaxPercentRate = 0.12m, Fee = 0.12m, Quantity = 5 };
            var item3 = new LineItem { Price = 88.45m, DiscountAmount = 10.78m, TaxPercentRate = 0.12m, Fee = 0.08m, Quantity = 12 };
            var payment = new PaymentIn { Price = 44.52m, DiscountAmount = 10, TaxPercentRate = 0.12m };
            var shipment = new Shipment { Price = 22.0m, DiscountAmount = 5m, TaxPercentRate = 0.12m };

            var order = new CustomerOrder
            {
                TaxPercentRate = 0.12m,
                Items = new List<LineItem> { item1, item2, item3 },
                InPayments = new List<PaymentIn> { payment },
                Shipments = new List<Shipment> { shipment }
            };

            var currency = new Currency
            {
                Code = order.Currency,
                RoundingPolicy = new DefaultMoneyRoundingPolicy()
            };

            var totalsCalculator = GetTotalsCalculator(currency);

            totalsCalculator.CalculateTotals(order);

            Assert.Equal(1400.00m, order.Total);

            order.Items.Clear();
            order.Shipments.Clear();
            order.InPayments.Clear();
            totalsCalculator.CalculateTotals(order);

            Assert.Equal(0m, order.Total);
        }

        [Fact]
        public void CalculateTotals_ShouldBe_RightTotals()
        {
            var item1 = new LineItem { Price = 10.99m, DiscountAmount = 1.33m, TaxPercentRate = 0.12m, Fee = 0.33m, Quantity = 2 };
            var item2 = new LineItem { Price = 55.22m, DiscountAmount = 5.89m, TaxPercentRate = 0.12m, Fee = 0.12m, Quantity = 5 };
            var item3 = new LineItem { Price = 88.45m, DiscountAmount = 10.78m, TaxPercentRate = 0.12m, Fee = 0.08m, Quantity = 12 };
            var payment = new PaymentIn { Price = 44.52m, DiscountAmount = 10, TaxPercentRate = 0.12m };
            var shipment = new Shipment { Price = 22.0m, DiscountAmount = 5m, TaxPercentRate = 0.12m, Fee = 20m };

            var order = new CustomerOrder
            {
                TaxPercentRate = 0.12m,
                Fee = 13.11m,
                Items = new List<LineItem> { item1, item2, item3 },
                InPayments = new List<PaymentIn> { payment },
                Shipments = new List<Shipment> { shipment }
            };

            var currency = new Currency
            {
                RoundingPolicy = new DefaultMoneyRoundingPolicy()
            };

            var totalsCalculator = GetTotalsCalculator(currency);

            totalsCalculator.CalculateTotals(order);

            Assert.Equal(12.3088m, item1.PriceWithTax);
            Assert.Equal(9.66m, item1.PlacedPrice);
            Assert.Equal(19.32m, item1.ExtendedPrice);
            Assert.Equal(1.4896m, item1.DiscountAmountWithTax);
            Assert.Equal(2.66m, item1.DiscountTotal);
            Assert.Equal(0.3696m, item1.FeeWithTax);
            Assert.Equal(10.8192m, item1.PlacedPriceWithTax);
            Assert.Equal(21.6384m, item1.ExtendedPriceWithTax);
            Assert.Equal(2.9792m, item1.DiscountTotalWithTax);
            Assert.Equal(2.3184m, item1.TaxTotal);

            Assert.Equal(5.6m, shipment.DiscountAmountWithTax);
            Assert.Equal(24.64m, shipment.PriceWithTax);
            Assert.Equal(22.40m, shipment.FeeWithTax);
            Assert.Equal(37.0m, shipment.Total);
            Assert.Equal(41.44m, shipment.TotalWithTax);
            Assert.Equal(4.44m, shipment.TaxTotal);

            Assert.Equal(34.52m, payment.Total);
            Assert.Equal(49.8624m, payment.PriceWithTax);
            Assert.Equal(38.6624m, payment.TotalWithTax);
            Assert.Equal(11.2m, payment.DiscountAmountWithTax);
            Assert.Equal(4.1424m, payment.TaxTotal);

            Assert.Equal(1359.48m, order.SubTotal);
            Assert.Equal(161.47m, order.SubTotalDiscount);
            Assert.Equal(180.85m, order.SubTotalDiscountWithTax);
            Assert.Equal(1522.62m, order.SubTotalWithTax);
            Assert.Equal(22.00m, order.ShippingSubTotal);
            Assert.Equal(24.64m, order.ShippingSubTotalWithTax);
            Assert.Equal(44.52m, order.PaymentSubTotal);
            Assert.Equal(49.86m, order.PaymentSubTotalWithTax);
            Assert.Equal(152.34m, order.TaxTotal);
            Assert.Equal(176.47m, order.DiscountTotal);
            Assert.Equal(197.65m, order.DiscountTotalWithTax);
            Assert.Equal(33.64m, order.FeeTotal);
            Assert.Equal(37.68m, order.FeeTotalWithTax);
            Assert.Equal(14.68m, order.FeeWithTax);
            Assert.Equal(1435.51m, order.Total);
        }
    }
}
