using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Services;
using VirtoCommerce.Platform.Core.DistributedLock;
using VirtoCommerce.StoreModule.Core.Services;
using Xunit;

namespace VirtoCommerce.OrdersModule.Tests
{
    [Trait("Category", "Unit")]
    public class PaymentFlowServiceTests
    {
        private readonly Mock<ICustomerOrderService> _customerOrderServiceMock = new();
        private readonly Mock<IPaymentService> _paymentServiceMock = new();
        private readonly Mock<IStoreService> _storeServiceMock = new();
        private readonly Mock<IValidator<OrderPaymentInfo>> _validatorMock = new();
        private readonly Mock<ITenantUniqueNumberGenerator> _numberGeneratorMock = new();
        private readonly Mock<IDistributedLockService> _lockServiceMock = new();

        private readonly PaymentFlowService _service;

        public PaymentFlowServiceTests()
        {
            var lockOptions = Options.Create(new PaymentDistributedLockOptions());

            _lockServiceMock
                .Setup(x => x.ExecuteAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<CaptureOrderPaymentResult>>>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<CancellationToken?>()))
                .Returns<string, Func<Task<CaptureOrderPaymentResult>>, TimeSpan?, TimeSpan?, TimeSpan?, CancellationToken?>(
                    (_, action, _, _, _, _) => action());

            _lockServiceMock
                .Setup(x => x.ExecuteAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<RefundOrderPaymentResult>>>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<CancellationToken?>()))
                .Returns<string, Func<Task<RefundOrderPaymentResult>>, TimeSpan?, TimeSpan?, TimeSpan?, CancellationToken?>(
                    (_, action, _, _, _, _) => action());

            _service = new PaymentFlowService(
                _customerOrderServiceMock.Object,
                _paymentServiceMock.Object,
                _storeServiceMock.Object,
                _validatorMock.Object,
                _numberGeneratorMock.Object,
                lockOptions,
                _lockServiceMock.Object);
        }

        public static IEnumerable<object[]> InvalidAmounts => new[]
        {
            new object[] { (decimal?)null },
            new object[] { 0m },
            new object[] { -10m },
        };

        [Theory]
        [MemberData(nameof(InvalidAmounts))]
        public async Task CapturePaymentAsync_AmountIsMissingOrNonPositive_ReturnsInvalidRequestError(decimal? amount)
        {
            var request = new CaptureOrderPaymentRequest
            {
                OrderId = "order-1",
                TransactionId = "txn-1",
                OuterId = "outer-1",
                Amount = amount,
            };

            var result = await _service.CapturePaymentAsync(request);

            result.Succeeded.Should().BeFalse();
            result.ErrorCode.Should().Be(PaymentFlowErrorCodes.InvalidRequestError);
            result.ErrorMessage.Should().Be(PaymentErrorDescriber.AmountRequired());

            _validatorMock.Verify(
                x => x.ValidateAsync(It.IsAny<ValidationContext<OrderPaymentInfo>>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _customerOrderServiceMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<IList<CustomerOrder>>()),
                Times.Never);
        }

        [Theory]
        [MemberData(nameof(InvalidAmounts))]
        public async Task RefundPaymentAsync_AmountIsMissingOrNonPositive_ReturnsInvalidRequestError(decimal? amount)
        {
            var request = new RefundOrderPaymentRequest
            {
                OrderId = "order-1",
                TransactionId = "txn-1",
                OuterId = "outer-1",
                Amount = amount,
            };

            var result = await _service.RefundPaymentAsync(request);

            result.Succeeded.Should().BeFalse();
            result.ErrorCode.Should().Be(PaymentFlowErrorCodes.InvalidRequestError);
            result.ErrorMessage.Should().Be(PaymentErrorDescriber.AmountRequired());

            _validatorMock.Verify(
                x => x.ValidateAsync(It.IsAny<ValidationContext<OrderPaymentInfo>>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _customerOrderServiceMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<IList<CustomerOrder>>()),
                Times.Never);
        }
    }
}
