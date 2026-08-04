using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Handlers;
using VirtoCommerce.OrdersModule.Data.Jobs;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;

namespace VirtoCommerce.OrdersModule.Tests
{
    // Any other test class that enqueues through the static BackgroundJob facade must join this collection:
    // the facade has no reset API (Initialize rejects null), so Dispose leaves a DISPOSED provider behind in
    // the static, and a class racing this one would see ObjectDisposedException from it.
    [Collection(nameof(BackgroundJobEnqueueTests))]
    public class BackgroundJobEnqueueTests
    {
        [Fact]
        public async Task LogChanges_LoggingEnabled_EnqueuesTheCollectedLogs()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var handler = new LogChangesOrderChangedEventHandler(
                Mock.Of<IChangeLogService>(),
                Mock.Of<IMemberService>(),
                CreateSettingsManager(enabled: true));

            var order = new CustomerOrder { Id = "order1", Number = "ORD-1" };
            var message = new OrderChangedEvent([new GenericChangedEntry<CustomerOrder>(order, order, EntryState.Added)]);

            //Act
            await handler.Handle(message);

            //Assert
            var payload = Assert.IsType<LogOrderChangesJobPayload>(capture.Payload);
            Assert.Equal(1, capture.EnqueueCount);
            Assert.NotEmpty(payload.OperationLogs);
            Assert.All(payload.OperationLogs, x => Assert.Equal("order1", x.ObjectId));
        }

        [Fact]
        public async Task LogChanges_LoggingDisabled_EnqueuesNothing()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var handler = new LogChangesOrderChangedEventHandler(
                Mock.Of<IChangeLogService>(),
                Mock.Of<IMemberService>(),
                CreateSettingsManager(enabled: false));

            var order = new CustomerOrder { Id = "order1", Number = "ORD-1" };

            //Act
            await handler.Handle(new OrderChangedEvent([new GenericChangedEntry<CustomerOrder>(order, order, EntryState.Added)]));

            //Assert
            Assert.Equal(0, capture.EnqueueCount);
        }

        [Fact]
        public async Task CancelPayment_OrderCancelled_EnqueuesOneArgumentPerPayment()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var handler = new CancelPaymentOrderChangedEventHandler(Mock.Of<ICustomerOrderService>());

            var oldOrder = new CustomerOrder { Id = "order1", IsCancelled = false, InPayments = [] };
            var newOrder = new CustomerOrder
            {
                Id = "order1",
                IsCancelled = true,
                InPayments = [new PaymentIn { Id = "payment1" }],
            };

            //Act
            await handler.Handle(new OrderChangedEvent([new GenericChangedEntry<CustomerOrder>(newOrder, oldOrder, EntryState.Modified)]));

            //Assert
            var payload = Assert.IsType<CancelPaymentJobPayload>(capture.Payload);
            Assert.Equal(1, capture.EnqueueCount);
            var argument = Assert.Single(payload.JobArguments);
            Assert.Equal("order1", argument.CustomerOrderId);
            Assert.Equal("payment1", argument.PaymentId);
        }

        [Fact]
        public async Task CancelPayment_NothingToCancel_EnqueuesNothing()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var handler = new CancelPaymentOrderChangedEventHandler(Mock.Of<ICustomerOrderService>());

            var order = new CustomerOrder { Id = "order1", IsCancelled = false, InPayments = [] };

            //Act
            await handler.Handle(new OrderChangedEvent([new GenericChangedEntry<CustomerOrder>(order, order, EntryState.Modified)]));

            //Assert
            Assert.Equal(0, capture.EnqueueCount);
        }

        [Fact]
        public async Task CancelPaymentJobHandler_DelegatesToTheEventHandler()
        {
            //Arrange
            // The job handlers are pure delegation: the logic stays on the event handler, which also remains the
            // target of background jobs enqueued by an earlier version. One of them is covered as a representative.
            var eventHandlerMock = new Mock<CancelPaymentOrderChangedEventHandler>(Mock.Of<ICustomerOrderService>());
            var jobArguments = new[] { new PaymentToCancelJobArgument { CustomerOrderId = "order1", PaymentId = "payment1" } };

            eventHandlerMock
                .Setup(x => x.TryToCancelOrderPaymentsAsync(jobArguments))
                .Returns(Task.CompletedTask);

            var handler = new CancelPaymentJobHandler(eventHandlerMock.Object);

            //Act
            await handler.Execute(new CancelPaymentJobPayload { JobArguments = jobArguments }, context: null,
                TestContext.Current.CancellationToken);

            //Assert
            eventHandlerMock.Verify(x => x.TryToCancelOrderPaymentsAsync(jobArguments), Times.Once);
        }

        private static ISettingsManager CreateSettingsManager(bool enabled)
        {
            var settingsManager = new Mock<ISettingsManager>();
            settingsManager
                .Setup(x => x.GetObjectSettingAsync(ModuleConstants.Settings.General.LogOrderChanges.Name, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ObjectSettingEntry { Value = enabled });

            return settingsManager.Object;
        }

        // Captures what a handler enqueued through the static BackgroundJob facade. IBackgroundJob is registered
        // Scoped here exactly as the engine module registers it, so this also proves the facade's per-call scope
        // resolves it - the handlers themselves are root-resolved and must never hold it.
        private sealed class EnqueueCapture : IDisposable
        {
            private readonly ServiceProvider _provider;

            public EnqueueCapture()
            {
                Setup<LogOrderChangesJobHandler>();
                Setup<CancelPaymentJobHandler>();

                var services = new ServiceCollection();
                services.AddScoped(_ => BackgroundJobMock.Object);
                _provider = services.BuildServiceProvider(validateScopes: true);

                BackgroundJob.Initialize(_provider);
            }

            public Mock<IBackgroundJob> BackgroundJobMock { get; } = new();

            public object Payload { get; private set; }

            public int EnqueueCount { get; private set; }

            public void Dispose()
            {
                _provider.Dispose();
            }

            private void Setup<THandler>()
                where THandler : class
            {
                BackgroundJobMock
                    .Setup(x => x.Enqueue<THandler>(It.IsAny<object>(), It.IsAny<EnqueueOptions>(), It.IsAny<CancellationToken>()))
                    .Callback<object, EnqueueOptions, CancellationToken>((payload, _, _) =>
                    {
                        Payload = payload;
                        EnqueueCount++;
                    })
                    .ReturnsAsync("job-id");
            }
        }
    }
}
