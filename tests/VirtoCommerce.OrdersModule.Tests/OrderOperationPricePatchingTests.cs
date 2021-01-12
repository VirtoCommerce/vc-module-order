using FluentAssertions;
using VirtoCommerce.OrdersModule.Data.Model;
using Xunit;

namespace VirtoCommerce.OrdersModule.Tests
{
    [Trait("Category", "CI")]
    public class OrderOperationPricePatchingTests
    {
        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(5, 1)]
        public void PatchPrices_Shipment_Needed(decimal sourcePrice, decimal patchedPrice)
        {
            // Arrange
            var sourceOperation = new ShipmentEntity()
            {
                Price = sourcePrice,
            };
            var patchedOperation = new ShipmentEntity()
            {
                Price = patchedPrice,
            };

            // Act
            sourceOperation.Patch(patchedOperation);

            // Assert
            patchedOperation.Price.Should().Be(sourcePrice);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(0, 0)]
        public void PatchPrices_Shipment_NotNeeded(decimal sourcePrice, decimal patchedPrice)
        {
            // Arrange
            var sourceOperation = new ShipmentEntity()
            {
                Price = sourcePrice,
            };
            var patchedOperation = new ShipmentEntity()
            {
                Price = patchedPrice,
            };

            // Act
            sourceOperation.Patch(patchedOperation);

            // Assert
            patchedOperation.Price.Should().Be(patchedPrice);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(5, 1)]
        public void PatchPrices_Payment_Needed(decimal sourcePrice, decimal patchedPrice)
        {
            // Arrange
            var sourceOperation = new PaymentInEntity()
            {
                Price = sourcePrice,
            };
            var patchedOperation = new PaymentInEntity()
            {
                Price = patchedPrice,
            };

            // Act
            sourceOperation.Patch(patchedOperation);

            // Assert
            patchedOperation.Price.Should().Be(sourcePrice);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(0, 0)]
        public void PatchPrices_Payment_NotNeeded(decimal sourcePrice, decimal patchedPrice)
        {
            // Arrange
            var sourceOperation = new PaymentInEntity()
            {
                Price = sourcePrice,
            };
            var patchedOperation = new PaymentInEntity()
            {
                Price = patchedPrice,
            };

            // Act
            sourceOperation.Patch(patchedOperation);

            // Assert
            patchedOperation.Price.Should().Be(patchedPrice);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(5, 1)]
        public void PatchPrices_LineItem_Needed(decimal sourcePrice, decimal patchedPrice)
        {
            // Arrange
            var sourceOperation = new LineItemEntity()
            {
                Price = sourcePrice,
            };
            var patchedOperation = new LineItemEntity()
            {
                Price = patchedPrice,
            };

            // Act
            sourceOperation.Patch(patchedOperation);

            // Assert
            patchedOperation.Price.Should().Be(sourcePrice);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(0, 0)]
        public void PatchPrices_LineItem_NotNeeded(decimal sourcePrice, decimal patchedPrice)
        {
            // Arrange
            var sourceOperation = new LineItemEntity()
            {
                Price = sourcePrice,
            };
            var patchedOperation = new LineItemEntity()
            {
                Price = patchedPrice,
            };

            // Act
            sourceOperation.Patch(patchedOperation);

            // Assert
            patchedOperation.Price.Should().Be(patchedPrice);
        }
    }
}
