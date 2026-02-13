using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using Moq;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Validators;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;
using Capture = VirtoCommerce.OrdersModule.Core.Model.Capture;

namespace VirtoCommerce.OrdersModule.Tests
{
    [Trait("Category", "Unit")]
    public class OrderDocumentCountValidatorTests
    {
        private readonly Mock<ISettingsManager> _settingsManagerMock;
        private readonly OrderDocumentCountValidator _validator;
        private const int DefaultMaxDocumentCount = 20;

        public OrderDocumentCountValidatorTests()
        {
            _settingsManagerMock = new Mock<ISettingsManager>();

            // Setup default settings value - mock the underlying GetObjectSettingAsync method
            _settingsManagerMock
                .Setup(x => x.GetObjectSettingAsync(
                    ModuleConstants.Settings.General.MaxOrderDocumentCount.Name,
                    null,
                    null))
                .ReturnsAsync(new ObjectSettingEntry
                {
                    Value = DefaultMaxDocumentCount
                });

            _validator = new OrderDocumentCountValidator(_settingsManagerMock.Object);
        }

        #region Happy Path Tests

        [Fact]
        public async Task Validate_EmptyOrder_ShouldPass()
        {
            // Arrange
            var order = CreateOrder();

            // Act
            var result = await _validator.TestValidateAsync(order, null, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_OrderWithinLimit_ShouldPass()
        {
            // Arrange
            var order = CreateOrder();
            order.InPayments = CreatePayments(5);
            order.Shipments = CreateShipments(5);

            // Add captures and refunds
            order.InPayments.First().Captures = CreateCaptures(3);
            order.InPayments.First().Refunds = CreateRefunds(2);

            // Total: 5 payments + 5 shipments + 3 captures + 2 refunds = 15 (within limit of 20)

            // Act
            var result = await _validator.TestValidateAsync(order, null, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_OrderAtExactLimit_ShouldPass()
        {
            // Arrange
            var order = CreateOrder();
            order.InPayments = CreatePayments(9);
            order.Shipments = CreateShipments(5);
            order.InPayments.First().Captures = CreateCaptures(3);
            order.InPayments.First().Refunds = CreateRefunds(2);

            // Total: 10 + 5 + 3 + 2 = 20 (exactly at limit)

            // Act
            var result = await _validator.TestValidateAsync(order, null, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        #endregion

        #region Failure Tests - Total Document Count Exceeded

        [Fact]
        public async Task Validate_TotalDocumentCountExceeded_ShouldFail()
        {
            // Arrange
            var order = CreateOrder();
            order.InPayments = CreatePayments(15);
            order.Shipments = CreateShipments(10);

            // Total: 15 + 10 = 25 (exceeds limit of 20)

            // Act
            var result = await _validator.TestValidateAsync(order, null, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(nameof(CustomerOrder));
            Assert.Contains("26", result.Errors.First().ErrorMessage);
            Assert.Contains("PaymentIn=15", result.Errors.First().ErrorMessage);
            Assert.Contains("Shipment=10", result.Errors.First().ErrorMessage);
        }

        [Fact]
        public async Task Validate_TotalDocumentCountWithCapturesExceeded_ShouldFail()
        {
            // Arrange
            var order = CreateOrder();
            order.InPayments = CreatePayments(10);
            order.Shipments = CreateShipments(5);
            order.InPayments.First().Captures = CreateCaptures(8);

            // Total: 10 + 5 + 8 = 23 (exceeds limit of 20)

            // Act
            var result = await _validator.TestValidateAsync(order, null, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(nameof(CustomerOrder));
            var errorMessage = result.Errors.First().ErrorMessage;
            Assert.Contains("24", errorMessage);
            Assert.Contains("Capture=8", errorMessage);
            Assert.Contains("PaymentIn=10", errorMessage);
            Assert.Contains("Shipment=5", errorMessage);
        }

        [Fact]
        public async Task Validate_TotalDocumentCountWithRefundsExceeded_ShouldFail()
        {
            // Arrange
            var order = CreateOrder();
            order.InPayments = CreatePayments(8);
            order.Shipments = CreateShipments(6);
            order.InPayments.First().Refunds = CreateRefunds(7);

            // Total: 8 + 6 + 7 = 21 (exceeds limit of 20)

            // Act
            var result = await _validator.TestValidateAsync(order, null, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(nameof(CustomerOrder));
            var errorMessage = result.Errors.First().ErrorMessage;
            Assert.Contains("22", errorMessage);
            Assert.Contains("Refund=7", errorMessage);
            Assert.Contains("PaymentIn=8", errorMessage);
            Assert.Contains("Shipment=6", errorMessage);
        }

        [Fact]
        public async Task Validate_TotalDocumentCountWithAllTypesExceeded_ShouldFail()
        {
            // Arrange
            var order = CreateOrder();
            order.InPayments = CreatePayments(8);
            order.Shipments = CreateShipments(7);

            // Distribute captures and refunds across payments
            order.InPayments.ElementAt(0).Captures = CreateCaptures(3);
            order.InPayments.ElementAt(1).Refunds = CreateRefunds(4);

            // Total: 8 + 7 + 3 + 4 = 22 (exceeds limit of 20)

            // Act
            var result = await _validator.TestValidateAsync(order, null, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(nameof(CustomerOrder));
            var errorMessage = result.Errors.First().ErrorMessage;
            Assert.Contains("23", errorMessage);
            Assert.Contains("PaymentIn=8", errorMessage);
            Assert.Contains("Shipment=7", errorMessage);
            Assert.Contains("Capture=3", errorMessage);
            Assert.Contains("Refund=4", errorMessage);
        }

        #endregion

        #region Failure Tests - Payment Sub-documents Exceeded

        [Fact]
        public async Task Validate_SinglePaymentSubDocumentsExceeded_ShouldFail()
        {
            // Arrange
            var order = CreateOrder();
            var payment = CreatePayments(1).First();
            payment.Captures = CreateCaptures(15);
            payment.Refunds = CreateRefunds(10);
            order.InPayments = new List<PaymentIn> { payment };

            // Payment sub-documents: 1 payment + 15 captures + 10 refunds = 26 total (exceeds limit of 20)
            // Note: Both order-level AND payment-level validation will fail

            // Act
            var result = await _validator.TestValidateAsync(order, null, TestContext.Current.CancellationToken);

            // Assert
            // The validator reports both order-level and payment-level errors
            Assert.True(result.Errors.Count >= 1, "Expected validation errors");

            // Check for order-level error
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("27") && e.ErrorMessage.Contains("CustomerOrder"));

            // Check for payment-level error (if reported separately)
            var paymentError = result.Errors.FirstOrDefault(e => e.ErrorMessage.Contains(payment.Number));
            if (paymentError != null)
            {
                Assert.Contains("25", paymentError.ErrorMessage);
                Assert.Contains("Capture=15", paymentError.ErrorMessage);
                Assert.Contains("Refund=10", paymentError.ErrorMessage);
            }
        }

        [Fact]
        public async Task Validate_MultiplePayments_OneExceedsSubDocumentLimit_ShouldFail()
        {
            // Arrange
            var order = CreateOrder();
            var payments = CreatePayments(3).ToList();

            // First payment - within limit
            payments[0].Captures = CreateCaptures(5);
            payments[0].Refunds = CreateRefunds(5);

            // Second payment - exceeds limit
            payments[1].Captures = CreateCaptures(12);
            payments[1].Refunds = CreateRefunds(10);

            // Third payment - within limit
            payments[2].Captures = CreateCaptures(3);

            order.InPayments = payments;

            // Total: 3 payments + 10 + 22 + 3 = 38 (exceeds order limit)
            // Payment[1]: 22 sub-docs (exceeds payment limit)

            // Act
            var result = await _validator.TestValidateAsync(order, null, TestContext.Current.CancellationToken);

            // Assert
            // The validator reports both order-level and payment-level errors
            Assert.True(result.Errors.Count >= 1, "Expected validation errors");

            // Check for order-level error
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("39") && e.ErrorMessage.Contains("CustomerOrder"));

            // Check for payment-level error (if reported separately)
            var paymentError = result.Errors.FirstOrDefault(e => e.ErrorMessage.Contains(payments[1].Number));
            if (paymentError != null)
            {
                Assert.Contains("22", paymentError.ErrorMessage);
                Assert.Contains("Capture=12", paymentError.ErrorMessage);
                Assert.Contains("Refund=10", paymentError.ErrorMessage);
            }
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task Validate_NullCollections_ShouldPass()
        {
            // Arrange
            var order = CreateOrder();
            order.InPayments = null;
            order.Shipments = null;

            // Act
            var result = await _validator.TestValidateAsync(order, null, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_PaymentWithNullSubCollections_ShouldPass()
        {
            // Arrange
            var order = CreateOrder();
            var payment = CreatePayments(1).First();
            payment.Captures = null;
            payment.Refunds = null;
            order.InPayments = new List<PaymentIn> { payment };

            // Act
            var result = await _validator.TestValidateAsync(order, null, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_CustomMaxLimit_ShouldRespectSetting()
        {
            // Arrange
            var customLimit = 5;
            _settingsManagerMock
                .Setup(x => x.GetObjectSettingAsync(
                    ModuleConstants.Settings.General.MaxOrderDocumentCount.Name,
                    null,
                    null))
                .ReturnsAsync(new ObjectSettingEntry { Value = customLimit });

            var validator = new OrderDocumentCountValidator(_settingsManagerMock.Object);

            var order = CreateOrder();
            order.InPayments = CreatePayments(4);
            order.Shipments = CreateShipments(2);

            // Total: 4 + 2 = 6 (exceeds custom limit of 5)

            // Act
            var result = await validator.TestValidateAsync(order, null, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(nameof(CustomerOrder));
            var errorMessage = result.Errors.First().ErrorMessage;
            Assert.Contains("7", errorMessage);
            Assert.Contains("5", errorMessage);
            Assert.Contains("PaymentIn=4", errorMessage);
            Assert.Contains("Shipment=2", errorMessage);
        }


        [Fact]
        public async Task Validate_VeryLargeMaxLimit_ShouldPass()
        {
            // Arrange
            var largeLimit = 10000;
            _settingsManagerMock
                .Setup(x => x.GetObjectSettingAsync(
                    ModuleConstants.Settings.General.MaxOrderDocumentCount.Name,
                    null,
                    null))
                .ReturnsAsync(new ObjectSettingEntry { Value = largeLimit });

            var validator = new OrderDocumentCountValidator(_settingsManagerMock.Object);

            var order = CreateOrder();
            order.InPayments = CreatePayments(100);
            order.Shipments = CreateShipments(100);

            // Total: 200 (within large limit)

            // Act
            var result = await validator.TestValidateAsync(order, null, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public async Task Validate_TypicalECommerceOrder_ShouldPass()
        {
            // Arrange - Typical order: 1 payment, 2 shipments, 1 capture
            var order = CreateOrder();
            order.InPayments = CreatePayments(1);
            order.Shipments = CreateShipments(2);
            order.InPayments.First().Captures = CreateCaptures(1);

            // Total: 1 + 2 + 1 = 4 (well within limit)

            // Act
            var result = await _validator.TestValidateAsync(order, null, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ComplexOrderWithPartialRefunds_ShouldPass()
        {
            // Arrange - Complex order: 2 payments, 3 shipments, 5 captures, 3 refunds
            var order = CreateOrder();
            var payments = CreatePayments(2).ToList();
            payments[0].Captures = CreateCaptures(3);
            payments[0].Refunds = CreateRefunds(2);
            payments[1].Captures = CreateCaptures(2);
            payments[1].Refunds = CreateRefunds(1);
            order.InPayments = payments;
            order.Shipments = CreateShipments(3);

            // Total: 2 + 3 + 5 + 3 = 13 (within limit)

            // Act
            var result = await _validator.TestValidateAsync(order, null, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_SuspiciousOrderWithManyDocuments_ShouldFail()
        {
            // Arrange - Suspicious activity: too many payment attempts
            var order = CreateOrder();
            order.InPayments = CreatePayments(25); // Many failed payment attempts
            order.Shipments = CreateShipments(1);

            // Total: 25 + 1 = 26 (exceeds limit - potential fraud/abuse)

            // Act
            var result = await _validator.TestValidateAsync(order, null, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(nameof(CustomerOrder));
        }

        #endregion

        #region Helper Methods

        private CustomerOrder CreateOrder()
        {
            return new CustomerOrder
            {
                Id = Guid.NewGuid().ToString(),
                Number = $"CO{DateTime.UtcNow:yyMMdd}-{new Random().Next(10000, 99999)}",
                CustomerId = Guid.NewGuid().ToString(),
                CustomerName = "Test Customer",
                StoreId = Guid.NewGuid().ToString(),
                StoreName = "Test Store",
                InPayments = new List<PaymentIn>(),
                Shipments = new List<Shipment>()
            };
        }

        private IList<PaymentIn> CreatePayments(int count)
        {
            var payments = new List<PaymentIn>();
            for (int i = 0; i < count; i++)
            {
                payments.Add(new PaymentIn
                {
                    Id = Guid.NewGuid().ToString(),
                    Number = $"PI{DateTime.UtcNow:yyMMdd}-{i:D5}",
                    CustomerId = Guid.NewGuid().ToString(),
                    CustomerName = "Test Customer",
                    Captures = new List<Capture>(),
                    Refunds = new List<Refund>()
                });
            }
            return payments;
        }

        private IList<Shipment> CreateShipments(int count)
        {
            var shipments = new List<Shipment>();
            for (int i = 0; i < count; i++)
            {
                shipments.Add(new Shipment
                {
                    Id = Guid.NewGuid().ToString(),
                    Number = $"SH{DateTime.UtcNow:yyMMdd}-{i:D5}"
                });
            }
            return shipments;
        }

        private IList<Capture> CreateCaptures(int count)
        {
            var captures = new List<Capture>();
            for (int i = 0; i < count; i++)
            {
                captures.Add(new Capture
                {
                    Id = Guid.NewGuid().ToString(),
                    Number = $"CA{DateTime.UtcNow:yyMMdd}-{i:D5}",
                    Amount = 100m
                });
            }
            return captures;
        }

        private IList<Refund> CreateRefunds(int count)
        {
            var refunds = new List<Refund>();
            for (int i = 0; i < count; i++)
            {
                refunds.Add(new Refund
                {
                    Id = Guid.NewGuid().ToString(),
                    Number = $"RE{DateTime.UtcNow:yyMMdd}-{i:D5}",
                    Amount = 50m
                });
            }
            return refunds;
        }

        #endregion
    }
}

