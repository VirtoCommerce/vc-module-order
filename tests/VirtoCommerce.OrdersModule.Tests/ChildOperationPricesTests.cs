using System.Linq;
using VirtoCommerce.OrdersModule.Core.Model;
using Xunit;

namespace VirtoCommerce.OrdersModule.Tests
{
    /// <summary>
    /// Payments and shipments are returned by their own endpoints, not only nested in an order, so
    /// reducing one on its own must zero every money field it carries and mark it as price-free.
    /// </summary>
    [Trait("Category", "CI")]
    public class ChildOperationPricesTests
    {
        private static string WithoutPrices =>
            (CustomerOrderResponseGroup.Full & ~CustomerOrderResponseGroup.WithPrices).ToString();

        private static PaymentIn GetPayment() => new()
        {
            Id = "paymentId",
            OrderId = "orderId",
            Sum = 2421250m,
            Total = 2421250m,
            TotalWithTax = 2421250m,
            Price = 2421250m,
            PriceWithTax = 2421250m,
            TaxTotal = 11m,
            TaxPercentRate = 0.2m,
            DiscountAmount = 5m,
            DiscountAmountWithTax = 6m,
        };

        private static Shipment GetShipment() => new()
        {
            Id = "shipmentId",
            CustomerOrderId = "orderId",
            Sum = 99m,
            Total = 99m,
            TotalWithTax = 99m,
            Price = 90m,
            PriceWithTax = 95m,
            Fee = 3m,
            FeeWithTax = 4m,
            TaxTotal = 5m,
            TaxPercentRate = 0.2m,
            DiscountAmount = 1m,
            DiscountAmountWithTax = 2m,
            Items =
            [
                new ShipmentItem
                {
                    Id = "shipmentItemId",
                    LineItem = new LineItem { Id = "lineItemId", Price = 50m, PriceWithTax = 60m, ExtendedPrice = 70m, PlacedPrice = 55m },
                },
            ],
            Packages =
            [
                new ShipmentPackage
                {
                    Id = "packageId",
                    Items =
                    [
                        new ShipmentItem
                        {
                            Id = "packedItemId",
                            LineItem = new LineItem { Id = "packedLineItemId", Price = 11m, PriceWithTax = 12m, ExtendedPrice = 13m },
                        },
                    ],
                },
            ],
            InPayments = [new PaymentIn { Id = "shipmentPaymentId", Sum = 42m, Total = 42m }],
        };

        [Fact]
        public void ReduceDetails_StandalonePayment_ZeroesEveryMoneyFieldAndClearsFlag()
        {
            var payment = GetPayment();

            payment.ReduceDetails(WithoutPrices);

            Assert.Equal(0m, payment.Sum);
            Assert.Equal(0m, payment.Total);
            Assert.Equal(0m, payment.TotalWithTax);
            Assert.Equal(0m, payment.Price);
            Assert.Equal(0m, payment.PriceWithTax);
            Assert.Equal(0m, payment.TaxTotal);
            Assert.Equal(0m, payment.TaxPercentRate);
            Assert.Equal(0m, payment.DiscountAmount);
            Assert.Equal(0m, payment.DiscountAmountWithTax);

            // The flag is what consumers read instead of re-checking the permission
            Assert.False(payment.WithPrices);
        }

        [Fact]
        public void ReduceDetails_StandaloneShipment_ZeroesEveryMoneyFieldAndClearsFlag()
        {
            var shipment = GetShipment();

            shipment.ReduceDetails(WithoutPrices);

            Assert.Equal(0m, shipment.Sum);
            Assert.Equal(0m, shipment.Total);
            Assert.Equal(0m, shipment.TotalWithTax);
            Assert.Equal(0m, shipment.Price);
            Assert.Equal(0m, shipment.PriceWithTax);
            Assert.Equal(0m, shipment.Fee);
            Assert.Equal(0m, shipment.FeeWithTax);
            Assert.Equal(0m, shipment.TaxTotal);
            Assert.Equal(0m, shipment.TaxPercentRate);
            Assert.Equal(0m, shipment.DiscountAmount);
            Assert.Equal(0m, shipment.DiscountAmountWithTax);

            Assert.False(shipment.WithPrices);
        }

        [Fact]
        public void ReduceDetails_Shipment_ZeroesNestedLineItemPrices()
        {
            var shipment = GetShipment();

            shipment.ReduceDetails(WithoutPrices);

            var item = shipment.Items.First().LineItem;
            Assert.Equal(0m, item.Price);
            Assert.Equal(0m, item.PriceWithTax);
            Assert.Equal(0m, item.ExtendedPrice);
            Assert.Equal(0m, item.PlacedPrice);
        }

        [Fact]
        public void ReduceDetails_Shipment_ZeroesLineItemPricesInsidePackages()
        {
            var shipment = GetShipment();

            shipment.ReduceDetails(WithoutPrices);

            var packed = shipment.Packages.First().Items.First().LineItem;
            Assert.Equal(0m, packed.Price);
            Assert.Equal(0m, packed.PriceWithTax);
            Assert.Equal(0m, packed.ExtendedPrice);
        }

        [Fact]
        public void ReduceDetails_Shipment_ZeroesNestedPayments()
        {
            var shipment = GetShipment();

            shipment.ReduceDetails(WithoutPrices);

            var payment = shipment.InPayments.First();
            Assert.Equal(0m, payment.Sum);
            Assert.Equal(0m, payment.Total);
            Assert.False(payment.WithPrices);
        }

        [Fact]
        public void RestoreDetails_Payment_RestoresEveryMoneyField()
        {
            var original = GetPayment();
            var reduced = GetPayment();
            reduced.ReduceDetails(WithoutPrices);

            reduced.RestoreDetails(original);

            Assert.Equal(2421250m, reduced.Sum);
            Assert.Equal(2421250m, reduced.Total);
            Assert.Equal(2421250m, reduced.Price);
            Assert.Equal(11m, reduced.TaxTotal);
            Assert.True(reduced.WithPrices);
        }

        [Fact]
        public void RestoreDetails_Shipment_RestoresOwnAndNestedPrices()
        {
            var original = GetShipment();
            var reduced = GetShipment();
            reduced.ReduceDetails(WithoutPrices);

            reduced.RestoreDetails(original);

            Assert.Equal(99m, reduced.Sum);
            Assert.Equal(90m, reduced.Price);
            Assert.True(reduced.WithPrices);

            Assert.Equal(50m, reduced.Items.First().LineItem.Price);
            Assert.Equal(70m, reduced.Items.First().LineItem.ExtendedPrice);
            Assert.Equal(11m, reduced.Packages.First().Items.First().LineItem.Price);

            Assert.Equal(42m, reduced.InPayments.First().Sum);
            Assert.True(reduced.InPayments.First().WithPrices);
        }
    }
}
