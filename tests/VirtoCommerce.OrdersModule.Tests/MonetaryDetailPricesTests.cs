using System.Linq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Tax;
using VirtoCommerce.OrdersModule.Core.Model;
using Xunit;

namespace VirtoCommerce.OrdersModule.Tests
{
    /// <summary>
    /// Discounts, tax details and fee details carry their own amounts. Leaving them populated lets a
    /// masked total be rebuilt from its breakdown, and they are reachable one level below the widget
    /// that reads the (correctly zeroed) scalar fields.
    /// </summary>
    [Trait("Category", "CI")]
    public class MonetaryDetailPricesTests
    {
        private static string WithoutPrices =>
            (CustomerOrderResponseGroup.Full & ~CustomerOrderResponseGroup.WithPrices).ToString();

        private static Discount Discount() => new() { Id = "d1", Coupon = "Coupon 50%", DiscountAmount = 44.995m, DiscountAmountWithTax = 49.99m };
        private static TaxDetail Tax() => new() { Name = "VAT", Amount = 12.5m, Rate = 0.2m };
        private static FeeDetail Fee() => new() { FeeId = "f1", Amount = 7.25m };

        private static CustomerOrder GetOrder() => new()
        {
            Id = "orderId",
            Total = 44.99m,
            SubTotal = 89.99m,
            Discounts = [Discount()],
            TaxDetails = [Tax()],
            FeeDetails = [Fee()],
            Items =
            [
                new LineItem
                {
                    Id = "itemId",
                    Price = 89.99m,
                    Discounts = [Discount()],
                    TaxDetails = [Tax()],
                    FeeDetails = [Fee()],
                },
            ],
            InPayments =
            [
                new PaymentIn { Id = "payId", Sum = 44.99m, Discounts = [Discount()], TaxDetails = [Tax()], FeeDetails = [Fee()] },
            ],
            Shipments =
            [
                new Shipment { Id = "shipId", Price = 10m, Discounts = [Discount()], TaxDetails = [Tax()], FeeDetails = [Fee()] },
            ],
        };

        [Theory]
        [InlineData("order")]
        [InlineData("lineItem")]
        [InlineData("payment")]
        [InlineData("shipment")]
        public void ReduceDetails_WithoutPrices_ZeroesDiscountAmountsAtEveryLevel(string level)
        {
            var order = GetOrder();

            order.ReduceDetails(WithoutPrices);

            var discounts = level switch
            {
                "order" => order.Discounts,
                "lineItem" => order.Items.First().Discounts,
                "payment" => order.InPayments.First().Discounts,
                _ => order.Shipments.First().Discounts,
            };

            var discount = discounts.First();
            Assert.Equal(0m, discount.DiscountAmount);
            Assert.Equal(0m, discount.DiscountAmountWithTax);

            // The descriptor stays: the user should still see that a coupon was applied
            Assert.Equal("Coupon 50%", discount.Coupon);
        }

        [Theory]
        [InlineData("order")]
        [InlineData("lineItem")]
        [InlineData("payment")]
        [InlineData("shipment")]
        public void ReduceDetails_WithoutPrices_ZeroesTaxAndFeeDetailAmountsAtEveryLevel(string level)
        {
            var order = GetOrder();

            order.ReduceDetails(WithoutPrices);

            var (taxes, fees) = level switch
            {
                "order" => (order.TaxDetails, order.FeeDetails),
                "lineItem" => (order.Items.First().TaxDetails, order.Items.First().FeeDetails),
                "payment" => (order.InPayments.First().TaxDetails, order.InPayments.First().FeeDetails),
                _ => (order.Shipments.First().TaxDetails, order.Shipments.First().FeeDetails),
            };

            Assert.Equal(0m, taxes.First().Amount);
            Assert.Equal(0m, taxes.First().Rate);
            Assert.Equal(0m, fees.First().Amount);
        }

        [Fact]
        public void ReduceDetails_WithoutPrices_LeavesNoNonZeroAmountAnywhere()
        {
            var order = GetOrder();

            order.ReduceDetails(WithoutPrices);

            var amounts = new[] { order.Discounts, order.Items.First().Discounts, order.InPayments.First().Discounts, order.Shipments.First().Discounts }
                .SelectMany(x => x)
                .SelectMany(d => new[] { d.DiscountAmount, d.DiscountAmountWithTax })
                .Concat(new[] { order.TaxDetails, order.Items.First().TaxDetails, order.InPayments.First().TaxDetails, order.Shipments.First().TaxDetails }
                    .SelectMany(x => x).SelectMany(t => new[] { t.Amount, t.Rate }))
                .Concat(new[] { order.FeeDetails, order.Items.First().FeeDetails, order.InPayments.First().FeeDetails, order.Shipments.First().FeeDetails }
                    .SelectMany(x => x).Select(f => f.Amount));

            Assert.All(amounts, x => Assert.Equal(0m, x));
        }

        [Fact]
        public void RestoreDetails_RestoresDiscountTaxAndFeeAmounts()
        {
            var original = GetOrder();
            var reduced = GetOrder();
            reduced.ReduceDetails(WithoutPrices);

            reduced.RestoreDetails(original);

            Assert.Equal(44.995m, reduced.Discounts.First().DiscountAmount);
            Assert.Equal(12.5m, reduced.TaxDetails.First().Amount);
            Assert.Equal(7.25m, reduced.FeeDetails.First().Amount);

            Assert.Equal(44.995m, reduced.Items.First().Discounts.First().DiscountAmount);
            Assert.Equal(44.995m, reduced.InPayments.First().Discounts.First().DiscountAmount);
            Assert.Equal(44.995m, reduced.Shipments.First().Discounts.First().DiscountAmount);
        }
    }
}
