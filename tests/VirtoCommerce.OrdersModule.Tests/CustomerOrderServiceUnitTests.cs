using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.OrdersModule.Data.Services;
using VirtoCommerce.PaymentModule.Core.Model.Search;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.ShippingModule.Core.Model.Search;
using VirtoCommerce.ShippingModule.Core.Services;
using VirtoCommerce.StoreModule.Core.Services;
using Xunit;

namespace VirtoCommerce.OrdersModule.Tests
{
    public class CustomerOrderServiceUnitTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IUniqueNumberGenerator> _uniqueNumberGeneratorMock;
        private readonly Mock<IStoreService> _storeServiceMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly Mock<ICustomerOrderTotalsCalculator> _customerOrderTotalsCalculatorMock;
        private readonly Mock<IShippingMethodsSearchService> _shippingMethodsSearchServiceMock;
        private readonly Mock<IPaymentMethodsSearchService> _paymentMethodsSearchServiceMock;

        public CustomerOrderServiceUnitTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _uniqueNumberGeneratorMock = new Mock<IUniqueNumberGenerator>();
            _storeServiceMock = new Mock<IStoreService>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _customerOrderTotalsCalculatorMock = new Mock<ICustomerOrderTotalsCalculator>();
            _shippingMethodsSearchServiceMock = new Mock<IShippingMethodsSearchService>();
            _paymentMethodsSearchServiceMock = new Mock<IPaymentMethodsSearchService>();
        }

        [Fact]
        public async Task GetByIdsAsync_GetThenSaveOrder_ReturnCachedOrder()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var newOrder = new CustomerOrder { Id = id };
            var newOrderEntity = (CustomerOrderEntity)AbstractTypeFactory<CustomerOrderEntity>.TryCreateInstance().FromModel(newOrder, new PrimaryKeyResolvingMap());
            var service = GetCustomerOrderServiceWithPlatformMemoryCache();
            _orderRepositoryMock.Setup(x => x.Add(newOrderEntity))
                .Callback(() =>
                {
                    _orderRepositoryMock.Setup(o => o.GetCustomerOrdersByIdsAsync(new[] { id }, null))
                        .ReturnsAsync(new[] { newOrderEntity });
                });

            //Act
            var nullOrder = await service.GetByIdAsync(id);
            await service.SaveChangesAsync(new[] { newOrder });
            var order = await service.GetByIdAsync(id);

            //Assert
            Assert.NotEqual(nullOrder, order);
        }

        [Fact]
        public async Task SaveChangesAsync_SaveNewOrder_TotalsCalculated()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var newOrder = new CustomerOrder
            {
                Id = id
            };
            var newOrderEntity = (CustomerOrderEntity)AbstractTypeFactory<CustomerOrderEntity>.TryCreateInstance().FromModel(newOrder, new PrimaryKeyResolvingMap());
            var service = GetCustomerOrderServiceWithPlatformMemoryCache();
            _orderRepositoryMock.Setup(x => x.Add(newOrderEntity))
                .Callback(() =>
                {
                    _orderRepositoryMock.Setup(o => o.GetCustomerOrdersByIdsAsync(new[] { id }, null))
                        .ReturnsAsync(new[] { newOrderEntity });
                });

            // Mock totals creation to verify it was called for change OrderChangeEvent.NewEntry
            // PaymentDiscountTotalWithTax property is ot stored in entity and filled only buy Totals calculator
            _customerOrderTotalsCalculatorMock.Setup(x => x.CalculateTotals(It.IsAny<CustomerOrder>()))
                .Callback<CustomerOrder>(x => x.PaymentDiscountTotalWithTax = 10);

            //Act
            await service.SaveChangesAsync(new[] { newOrder });

            //Assert
            _customerOrderTotalsCalculatorMock.Verify(x => x.CalculateTotals(It.Is<CustomerOrder>(order => order == newOrder)), Times.Exactly(2));
            // Here we are ensuring that totals calculation was done for OrderChangeEvent.NewEntry
            _eventPublisherMock.Verify(x => x.Publish(
                It.Is<OrderChangeEvent>(changedEvent => changedEvent.ChangedEntries.First().NewEntry.PaymentDiscountTotalWithTax == 10), It.IsAny<CancellationToken>())
            , Times.Once);
        }

        [Fact]
        public async Task SaveChangesAsync_UpdatingExistingOrder_TotalsCalculated()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var newOrder = new CustomerOrder
            {
                Id = id
            };
            var newOrderEntity = (CustomerOrderEntity)AbstractTypeFactory<CustomerOrderEntity>.TryCreateInstance().FromModel(newOrder, new PrimaryKeyResolvingMap());
            var service = GetCustomerOrderServiceWithPlatformMemoryCache();

            _orderRepositoryMock.Setup(o => o.GetCustomerOrdersByIdsAsync(new[] { id }, It.IsAny<string>()))
                .ReturnsAsync(new[] { newOrderEntity });

            // Mock CalculateTotals to verify it was called for OldEntry and NewEntry in the OrderChangedEvent
            // PaymentDiscountTotalWithTax property is ot stored in entity and filled only buy Totals calculator
            _customerOrderTotalsCalculatorMock.Setup(x => x.CalculateTotals(It.IsAny<CustomerOrder>()))
                .Callback<CustomerOrder>(x => x.PaymentDiscountTotalWithTax = 10);

            //Act
            await service.SaveChangesAsync(new[] { newOrder });

            //Assert
            _customerOrderTotalsCalculatorMock.Verify(x => x.CalculateTotals(It.Is<CustomerOrder>(order => order == newOrder)), Times.Exactly(3));

            // Ensure CalculateTotals is called for OldEntry
            _eventPublisherMock.Verify(x =>
                x.Publish(It.Is<OrderChangedEvent>(changedEvent =>
                        changedEvent.ChangedEntries.First().OldEntry.PaymentDiscountTotalWithTax == 10),
                    It.IsAny<CancellationToken>()), Times.Once);

            // Ensure CalculateTotals is called for NewEntry
            _eventPublisherMock.Verify(x =>
                x.Publish(It.Is<OrderChangedEvent>(changedEvent =>
                    changedEvent.ChangedEntries.First().NewEntry.PaymentDiscountTotalWithTax == 10),
                    It.IsAny<CancellationToken>()), Times.Once);
        }


        private CustomerOrderService GetCustomerOrderServiceWithPlatformMemoryCache()
        {
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);
            _orderRepositoryMock.Setup(ss => ss.UnitOfWork).Returns(_unitOfWorkMock.Object);

            return GetCustomerOrderService(platformMemoryCache, _orderRepositoryMock.Object);
        }

        private CustomerOrderService GetCustomerOrderService(IPlatformMemoryCache platformMemoryCache, IOrderRepository orderRepository)
        {
            _shippingMethodsSearchServiceMock
                .Setup(x => x.SearchShippingMethodsAsync(It.IsAny<ShippingMethodsSearchCriteria>()))
                .ReturnsAsync(new ShippingMethodsSearchResult());

            _paymentMethodsSearchServiceMock
                .Setup(x => x.SearchPaymentMethodsAsync(It.IsAny<PaymentMethodsSearchCriteria>()))
                .ReturnsAsync(new PaymentMethodsSearchResult());

            return new CustomerOrderService(() => orderRepository,
                _uniqueNumberGeneratorMock.Object,
                _storeServiceMock.Object,
                _eventPublisherMock.Object,
                _customerOrderTotalsCalculatorMock.Object,
                _shippingMethodsSearchServiceMock.Object,
                _paymentMethodsSearchServiceMock.Object,
                platformMemoryCache);
        }
    }
}
