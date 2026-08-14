using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Handlers;
using VirtoCommerce.OrdersModule.Data.Jobs;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using Xunit;

namespace VirtoCommerce.OrdersModule.Tests
{
    // Any other test class that enqueues through the static BackgroundJob facade must join this collection:
    // the facade has no reset API (Initialize rejects null), so Dispose leaves a DISPOSED provider behind in
    // the static, and a class racing this one would see ObjectDisposedException from it.
    [Collection(nameof(BackgroundJobEnqueueTests))]
    public class BackgroundJobEnqueueTests
    {
        // Mirrors JobJsonSettings.Default of the BackgroundJobs module: no TypeNameHandling, which is precisely what
        // every payload has to survive. The order module depends on the Platform.Core.Jobs abstractions only, so the
        // settings are restated here rather than referenced.
        private static readonly JsonSerializerSettings JobSerializerSettings = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Include,
        };

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
        public void AdjustInventory_PayloadCarriesNothingPolymorphic_AndSurvivesJobSerialization()
        {
            //Arrange
            // The engine serializes the payload with no TypeNameHandling, so anything polymorphic reachable from it
            // (PaymentIn.PaymentMethod, Shipment.ShippingMethod, IOperation.ChildrenOperations) is written without a
            // $type and cannot be read back on the worker. The payload must therefore carry a projection, not the order.
            var payload = AdjustInventoryJobPayload.FromChangedEntry(CreateChangedEntry());

            //Act
            var json = JsonConvert.SerializeObject(payload, JobSerializerSettings);
            var restored = JsonConvert.DeserializeObject<AdjustInventoryJobPayload>(json, JobSerializerSettings);

            //Assert
            var changedEntry = restored.ToChangedEntry();
            Assert.Equal(EntryState.Modified, changedEntry.EntryState);
            Assert.Equal("order1", changedEntry.NewEntry.Id);
            Assert.Equal("store1", changedEntry.NewEntry.StoreId);
            Assert.Equal(ModuleConstants.CustomerOrderStatus.Cancelled, changedEntry.NewEntry.Status);
            Assert.Equal("New", changedEntry.OldEntry.Status);

            var newItem = Assert.Single(changedEntry.NewEntry.Items);
            Assert.Equal("item1", newItem.Id);
            Assert.Equal("product1", newItem.ProductId);
            Assert.Equal(3, newItem.Quantity);

            var oldItem = Assert.Single(changedEntry.OldEntry.Items);
            Assert.Equal("item1", oldItem.Id);
            Assert.Equal("product1", oldItem.ProductId);
            Assert.Equal(2, oldItem.Quantity);
        }

        [Fact]
        public async Task AdjustInventory_ReleasesCancelledItems_AfterThePayloadRoundTrip()
        {
            //Arrange
            // Proves the projection carries everything ProcessInventoryChanges reads: the payload goes through the
            // real enqueue path and the engine's serialization before the job handler runs it.
            using var capture = new EnqueueCapture();
            var reservationServiceMock = new Mock<IInventoryReservationService>();
            var eventHandler = CreateAdjustInventoryHandler(reservationServiceMock);

            //Act
            await eventHandler.Handle(new OrderChangedEvent([CreateChangedEntry()]));

            var payload = Assert.IsType<AdjustInventoryJobPayload>(capture.Payload);
            var json = JsonConvert.SerializeObject(payload, JobSerializerSettings);
            var restored = JsonConvert.DeserializeObject<AdjustInventoryJobPayload>(json, JobSerializerSettings);

            await new AdjustInventoryJobHandler(eventHandler).Execute(restored, context: null, TestContext.Current.CancellationToken);

            //Assert
            reservationServiceMock.Verify(x => x.ReleaseAsync(It.Is<InventoryReleaseRequest>(request =>
                request.ParentId == "order1" &&
                request.Items.Count == 1 &&
                request.Items[0].ItemId == "item1" &&
                request.Items[0].ProductId == "product1")), Times.Once);
        }

        [Fact]
        public async Task AdjustInventory_ReservesOrderedItems_AfterThePayloadRoundTrip()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var reservationServiceMock = new Mock<IInventoryReservationService>();
            var eventHandler = CreateAdjustInventoryHandler(reservationServiceMock);

            var newOrder = CreateOrder("New", quantity: 3);

            //Act
            await eventHandler.Handle(new OrderChangedEvent([new GenericChangedEntry<CustomerOrder>(newOrder, EntryState.Added)]));

            var payload = Assert.IsType<AdjustInventoryJobPayload>(capture.Payload);
            var json = JsonConvert.SerializeObject(payload, JobSerializerSettings);
            var restored = JsonConvert.DeserializeObject<AdjustInventoryJobPayload>(json, JobSerializerSettings);

            await new AdjustInventoryJobHandler(eventHandler).Execute(restored, context: null, TestContext.Current.CancellationToken);

            //Assert
            reservationServiceMock.Verify(x => x.ReserveAsync(It.Is<InventoryReserveRequest>(request =>
                request.ParentId == "order1" &&
                request.FulfillmentCenterIds.Contains("fulfillmentCenter1") &&
                request.Items.Count == 1 &&
                request.Items[0].ItemId == "item1" &&
                request.Items[0].ProductId == "product1" &&
                request.Items[0].Quantity == 3)), Times.Once);
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
            return CreateSettingsManager(ModuleConstants.Settings.General.LogOrderChanges.Name, enabled);
        }

        private static ISettingsManager CreateSettingsManager(string settingName, bool enabled)
        {
            var settingsManager = new Mock<ISettingsManager>();
            settingsManager
                .Setup(x => x.GetObjectSettingAsync(settingName, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ObjectSettingEntry { Value = enabled });

            return settingsManager.Object;
        }

        private static AdjustInventoryOrderChangedEventHandler CreateAdjustInventoryHandler(Mock<IInventoryReservationService> reservationServiceMock)
        {
            var storeServiceMock = new Mock<IStoreService>();
            storeServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), false))
                .ReturnsAsync([new Store { Id = "store1", MainFulfillmentCenterId = "fulfillmentCenter1" }]);

            var itemServiceMock = new Mock<IItemService>();
            itemServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), false))
                .ReturnsAsync([new CatalogProduct { Id = "product1", TrackInventory = true }]);

            return new AdjustInventoryOrderChangedEventHandler(
                storeServiceMock.Object,
                CreateSettingsManager(ModuleConstants.Settings.General.OrderAdjustInventory.Name, enabled: true),
                itemServiceMock.Object,
                reservationServiceMock.Object,
                Mock.Of<ILogger<AdjustInventoryOrderChangedEventHandler>>());
        }

        // A cancelled order: the quantity ordered before cancellation has to be released.
        private static GenericChangedEntry<CustomerOrder> CreateChangedEntry()
        {
            return new GenericChangedEntry<CustomerOrder>(
                CreateOrder(ModuleConstants.CustomerOrderStatus.Cancelled, quantity: 3),
                CreateOrder("New", quantity: 2),
                EntryState.Modified);
        }

        private static CustomerOrder CreateOrder(string status, int quantity)
        {
            var payment = new PaymentIn { Id = "payment1", PaymentMethod = new TestPaymentMethod() };

            var order = new CustomerOrder
            {
                Id = "order1",
                StoreId = "store1",
                Status = status,
                Items = [new LineItem { Id = "item1", ProductId = "product1", Quantity = quantity }],
                InPayments = [payment],
            };

            // OperationEntity.ToModel fills this on every order read from the database. It is an IOperation
            // collection, so it is the second member of the graph that no type-less serializer can read back.
            order.ChildrenOperations = [payment];

            return order;
        }

        // PaymentIn.PaymentMethod is an abstract type filled by CustomerOrderService.LoadOrderDependenciesAsync,
        // which is what the payload used to drag into the job store.
        private sealed class TestPaymentMethod() : PaymentMethod("test")
        {
            public override PaymentMethodType PaymentMethodType => PaymentMethodType.Unknown;

            public override PaymentMethodGroupType PaymentMethodGroupType => PaymentMethodGroupType.Alternative;
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
                Setup<AdjustInventoryJobHandler>();

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
