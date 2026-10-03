using System.Linq;
using VirtoCommerce.OrdersModule.Core.Model;
using Xunit;

namespace VirtoCommerce.OrdersModule.Tests
{
    [Trait("Category", "CI")]
    public class NestedOperationPricesTests
    {
        private static string WithoutPrices =>
            (CustomerOrderResponseGroup.Full & ~CustomerOrderResponseGroup.WithPrices).ToString();

        private static CustomerOrder GetOrder()
        {
            return new CustomerOrder
            {
                Id = "orderId",
                Total = 10m,
                Sum = 10m,
                InPayments =
                [
                    new PaymentIn
                    {
                        Id = "paymentId",
                        Sum = 10m,
                        Total = 10m,
                        Captures = [new Capture { Id = "captureId", Amount = 7m, Sum = 7m }],
                        Refunds = [new Refund { Id = "refundId", Amount = 3m, Sum = 3m }],
                    },
                ],
            };
        }

        [Fact]
        public void ReduceDetails_WithoutPrices_ZeroesNestedCaptureAndRefund()
        {
            var order = GetOrder();

            order.ReduceDetails(WithoutPrices);

            var payment = order.InPayments.First();
            var capture = payment.Captures.First();
            var refund = payment.Refunds.First();

            Assert.Equal(0m, capture.Amount);
            Assert.Equal(0m, capture.Sum);
            Assert.False(capture.WithPrices);

            Assert.Equal(0m, refund.Amount);
            Assert.Equal(0m, refund.Sum);
            Assert.False(refund.WithPrices);
        }

        [Fact]
        public void ReduceDetails_WithoutPrices_KeepsCaptureAndRefundCollections()
        {
            var order = GetOrder();

            order.ReduceDetails(WithoutPrices);

            // Full & ~WithPrices keeps WithCaptures/WithRefunds, so the collections must survive
            // with their non-price data intact.
            var payment = order.InPayments.First();
            Assert.Single(payment.Captures);
            Assert.Single(payment.Refunds);
            Assert.Equal("captureId", payment.Captures.First().Id);
            Assert.Equal("refundId", payment.Refunds.First().Id);
        }

        [Fact]
        public void RestoreDetails_RestoresNestedCaptureAndRefundAmounts()
        {
            var original = GetOrder();
            var reduced = GetOrder();
            reduced.ReduceDetails(WithoutPrices);

            reduced.RestoreDetails(original);

            var payment = reduced.InPayments.First();
            var capture = payment.Captures.First();
            var refund = payment.Refunds.First();

            Assert.Equal(7m, capture.Amount);
            Assert.Equal(7m, capture.Sum);
            Assert.True(capture.WithPrices);

            Assert.Equal(3m, refund.Amount);
            Assert.Equal(3m, refund.Sum);
            Assert.True(refund.WithPrices);
        }
    }
}
