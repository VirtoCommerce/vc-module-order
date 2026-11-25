using System.Collections.Generic;
using System.Linq;
using Moq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Services;
using Xunit;

namespace VirtoCommerce.OrdersModule.Tests
{
    // VCST-6824: a line total is rounded once from the exact unit price, so the order line and the order total
    // match the unit price shown in the catalog and the cart under any currency rounding policy
    public class OrderLineTotalsRoundingTests
    {
        [Theory]
        //           RoundingType,  Digits, Price,   SalePrice, Tax,   Qty, ExtendedPriceWithTax, OrderTotal
        [InlineData("Rounding001",    2,    12.5549,   6.52,    0.20,   1,        7.82,              7.82)]
        [InlineData("Rounding001",    2,    12.5649,   6.53,    0.21,   1,        7.90,              7.90)]
        [InlineData("Rounding001",    0,    10.50,     7.20,    0.20,   1,        9,                 9)]
        [InlineData("Rounding005Up",  2,    19.99,    14.97,    0.081,  3,       48.55,             48.55)]
        [InlineData("Rounding001",    2,    20.00,    15.00,    0.20,   2,       36.00,             36.00)]
        public void CalculateTotals_SingleLine_LineAndOrderTotalMatchUnitPrice(string roundingType, int digits, decimal price, decimal salePrice, decimal tax, int quantity,
            decimal expectedExtendedPriceWithTax, decimal expectedOrderTotal)
        {
            // Arrange
            var currency = CreateCurrency(roundingType, digits);
            var lineItem = CreateLineItem(price, salePrice, tax, quantity);
            var order = new CustomerOrder { Currency = currency.Code, Items = new List<LineItem> { lineItem } };

            // Act
            CreateCalculator(currency).CalculateTotals(order);

            // Assert
            Assert.Equal(expectedExtendedPriceWithTax, lineItem.ExtendedPriceWithTax);
            Assert.Equal(expectedOrderTotal, order.Total);
            Assert.Equal(lineItem.ExtendedPrice, lineItem.ListTotal - lineItem.DiscountTotal);
            Assert.Equal(lineItem.ExtendedPriceWithTax, lineItem.ExtendedPrice + lineItem.TaxTotal);
        }

        [Fact]
        public void CalculateTotals_SeveralLines_OrderTotalIsSumOfLines()
        {
            // Arrange
            var currency = CreateCurrency("Rounding001", 2);
            var order = new CustomerOrder
            {
                Currency = currency.Code,
                Items = new List<LineItem>
                {
                    CreateLineItem(12.5549m, 6.52m, 0.20m, 3),
                    CreateLineItem(20.00m, 15.00m, 0.20m, 2),
                    CreateLineItem(10.0049m, 8.0049m, 0.20m, 5),
                    CreateLineItem(12.5649m, 6.53m, 0.20m, 7),
                },
            };

            // Act
            CreateCalculator(currency).CalculateTotals(order);

            // Assert
            Assert.Equal(order.Items.Sum(x => x.ListTotal), order.SubTotal);
            Assert.Equal(order.Items.Sum(x => x.ExtendedPrice), order.SubTotal - order.SubTotalDiscount);
            Assert.Equal(order.Items.Sum(x => x.ExtendedPriceWithTax), order.Total);
        }

        private static LineItem CreateLineItem(decimal price, decimal salePrice, decimal tax, int quantity)
        {
            return new LineItem
            {
                Price = price,
                DiscountAmount = price - salePrice,
                TaxPercentRate = tax,
                Quantity = quantity,
                Currency = "TST",
            };
        }

        private static Currency CreateCurrency(string roundingType, int digits)
        {
            return new Currency(new Language("en-US"), code: "TST")
            {
                DecimalDigits = digits,
                RoundingType = roundingType,
                MidpointRounding = "AwayFromZero",
                RoundingPolicy = new DefaultMoneyRoundingPolicy(),
            };
        }

        private static DefaultCustomerOrderTotalsCalculator CreateCalculator(Currency currency)
        {
            var currencyServiceMock = new Mock<ICurrencyService>();
            currencyServiceMock.Setup(x => x.GetAllCurrenciesAsync()).ReturnsAsync([currency]);
            return new DefaultCustomerOrderTotalsCalculator(currencyServiceMock.Object);
        }
    }
}
