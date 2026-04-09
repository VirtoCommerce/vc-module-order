using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Validators;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;

namespace VirtoCommerce.OrdersModule.Tests
{
    [Trait("Category", "Unit")]
    public class CustomerOrderValidatorTests
    {
        private readonly Mock<ISettingsManager> _settingsManagerMock;
        private readonly Mock<IValidator<LineItem>> _lineItemValidatorMock;
        private readonly Mock<IValidator<Shipment>> _shipmentValidatorMock;
        private readonly Mock<IValidator<PaymentIn>> _paymentInValidatorMock;
        private readonly Mock<IValidator<IOperation>> _operationValidatorMock;

        public CustomerOrderValidatorTests()
        {
            _settingsManagerMock = new Mock<ISettingsManager>();
            _lineItemValidatorMock = new Mock<IValidator<LineItem>>();
            _shipmentValidatorMock = new Mock<IValidator<Shipment>>();
            _paymentInValidatorMock = new Mock<IValidator<PaymentIn>>();
            _operationValidatorMock = new Mock<IValidator<IOperation>>();

            // Setup default validators to return success
            _lineItemValidatorMock
                .Setup(x => x.Validate(It.IsAny<ValidationContext<LineItem>>()))
                .Returns(new ValidationResult());

            _shipmentValidatorMock
                .Setup(x => x.Validate(It.IsAny<ValidationContext<Shipment>>()))
                .Returns(new ValidationResult());

            _paymentInValidatorMock
                .Setup(x => x.Validate(It.IsAny<ValidationContext<PaymentIn>>()))
                .Returns(new ValidationResult());

            _operationValidatorMock
                .Setup(x => x.Validate(It.IsAny<ValidationContext<IOperation>>()))
                .Returns(new ValidationResult());
        }

        [Fact]
        public async Task ValidateAsync_WhenValidationDisabled_ShouldSkipValidation()
        {
            // Arrange
            _settingsManagerMock
                .Setup(x => x.GetObjectSettingAsync(
                    ModuleConstants.Settings.General.CustomerOrderValidation.Name,
                    null,
                    null))
                .ReturnsAsync(new ObjectSettingEntry { Value = false });

            var validator = CreateValidator();
            var order = CreateInvalidOrder(); // Order that would fail validation

            // Act
            var result = await validator.ValidateAsync(order, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsValid, "Validation should be skipped when disabled");
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task ValidateAsync_WhenValidationEnabled_ShouldPerformValidation()
        {
            // Arrange
            _settingsManagerMock
                .Setup(x => x.GetObjectSettingAsync(
                    ModuleConstants.Settings.General.CustomerOrderValidation.Name,
                    null,
                    null))
                .ReturnsAsync(new ObjectSettingEntry { Value = true });

            var validator = CreateValidator();
            var order = CreateInvalidOrder(); // Order that would fail validation

            // Act
            var result = await validator.ValidateAsync(order, TestContext.Current.CancellationToken);

            // Assert
            Assert.False(result.IsValid, "Validation should run when enabled");
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Validate_WhenValidationDisabled_ShouldSkipValidation()
        {
            // Arrange
            _settingsManagerMock
                .Setup(x => x.GetObjectSettingAsync(
                    ModuleConstants.Settings.General.CustomerOrderValidation.Name,
                    null,
                    null))
                .ReturnsAsync(new ObjectSettingEntry { Value = false });

            var validator = CreateValidator();
            var order = CreateInvalidOrder(); // Order that would fail validation

            // Act
            var result = validator.Validate(order);

            // Assert
            Assert.True(result.IsValid, "Synchronous validation should be skipped when disabled");
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_WhenValidationEnabled_ShouldPerformValidation()
        {
            // Arrange
            _settingsManagerMock
                .Setup(x => x.GetObjectSettingAsync(
                    ModuleConstants.Settings.General.CustomerOrderValidation.Name,
                    null,
                    null))
                .ReturnsAsync(new ObjectSettingEntry { Value = true });

            var validator = CreateValidator();
            var order = CreateInvalidOrder(); // Order that would fail validation

            // Act
            var result = validator.Validate(order);

            // Assert
            Assert.False(result.IsValid, "Synchronous validation should run when enabled");
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public async Task ValidateAsync_ValidOrder_WhenValidationEnabled_ShouldPass()
        {
            // Arrange
            _settingsManagerMock
                .Setup(x => x.GetObjectSettingAsync(
                    ModuleConstants.Settings.General.CustomerOrderValidation.Name,
                    null,
                    null))
                .ReturnsAsync(new ObjectSettingEntry { Value = true });

            var validator = CreateValidator();
            var order = CreateValidOrder();

            // Act
            var result = await validator.ValidateAsync(order, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsValid, "Valid order should pass validation");
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_ValidOrder_WhenValidationEnabled_ShouldPass()
        {
            // Arrange
            _settingsManagerMock
                .Setup(x => x.GetObjectSettingAsync(
                    ModuleConstants.Settings.General.CustomerOrderValidation.Name,
                    null,
                    null))
                .ReturnsAsync(new ObjectSettingEntry { Value = true });

            var validator = CreateValidator();
            var order = CreateValidOrder();

            // Act
            var result = validator.Validate(order);

            // Assert
            Assert.True(result.IsValid, "Valid order should pass synchronous validation");
            Assert.Empty(result.Errors);
        }

        #region Helper Methods

        private CustomerOrderValidator CreateValidator()
        {
            return new CustomerOrderValidator(
                _settingsManagerMock.Object,
                new[] { _lineItemValidatorMock.Object },
                new[] { _shipmentValidatorMock.Object },
                _paymentInValidatorMock.Object,
                new[] { _operationValidatorMock.Object });
        }

        private CustomerOrder CreateValidOrder()
        {
            return new CustomerOrder
            {
                Id = "test-order-id",
                Number = "ORDER-001",
                CustomerId = "customer-id",
                CustomerName = "John Doe",
                StoreId = "store-id",
                StoreName = "Test Store",
                Currency = "USD",
                Items = new List<LineItem>(),
                InPayments = new List<PaymentIn>(),
                Shipments = new List<Shipment>()
            };
        }

        private CustomerOrder CreateInvalidOrder()
        {
            return new CustomerOrder
            {
                Id = "test-order-id",
                Number = "", // Invalid: empty number
                CustomerId = "", // Invalid: empty customer ID
                CustomerName = "", // Invalid: empty customer name
                StoreId = "", // Invalid: empty store ID
                Currency = "USD",
                Items = new List<LineItem>(),
                InPayments = new List<PaymentIn>(),
                Shipments = new List<Shipment>()
            };
        }

        #endregion
    }
}

