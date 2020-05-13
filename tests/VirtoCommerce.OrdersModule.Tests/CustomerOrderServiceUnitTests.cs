using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using VirtoCommerce.CoreModule.Core.Common;
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
        private readonly Mock<IPlatformMemoryCache> _platformMemoryCacheMock;
        private readonly Mock<ICacheEntry> _cacheEntryMock;

        public CustomerOrderServiceUnitTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _orderRepositoryMock.Setup(ss => ss.UnitOfWork).Returns(_unitOfWorkMock.Object);
            _uniqueNumberGeneratorMock = new Mock<IUniqueNumberGenerator>();
            _storeServiceMock = new Mock<IStoreService>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _customerOrderTotalsCalculatorMock = new Mock<ICustomerOrderTotalsCalculator>();
            _shippingMethodsSearchServiceMock = new Mock<IShippingMethodsSearchService>();
            _paymentMethodsSearchServiceMock = new Mock<IPaymentMethodsSearchService>();
            _platformMemoryCacheMock = new Mock<IPlatformMemoryCache>();
            _cacheEntryMock = new Mock<ICacheEntry>();
            
        }

        [Fact]
        public async Task GetByIdsAsync_GetThenSaveOrder_ReturnCachedOrder()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var fakeOrder = GetFakeCustomerOrder(id);
            var fakeOrderEntity = (CustomerOrderEntity)AbstractTypeFactory<CustomerOrderEntity>.TryCreateInstance().FromModel(fakeOrder, new PrimaryKeyResolvingMap());
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);
            var service = GetCustomerOrderService(platformMemoryCache);
            _cacheEntryMock.SetupGet(c => c.ExpirationTokens).Returns(new List<IChangeToken>());
            var cacheKey = CacheKey.With(service.GetType(), nameof(service.GetByIdsAsync), string.Join("-", new[] { id }), null);

            //Act
            var nullOrder = await service.GetByIdAsync(id);
            var cachedNullOrder = (CustomerOrder[])platformMemoryCache.Get(cacheKey);
            await service.SaveChangesAsync(new[] { fakeOrder });
            _orderRepositoryMock
                .Setup(x => x.GetCustomerOrdersByIdsAsync(new[] {id}, null))
                .ReturnsAsync(new [] { fakeOrderEntity });
            var order = await service.GetByIdAsync(id);
            //need to call again for verify calling times
            await service.GetByIdAsync(id);
            var cachedOrder = (CustomerOrder[])platformMemoryCache.Get(cacheKey);

            //Assert
            Assert.Null(nullOrder);
            Assert.Empty(cachedNullOrder);
            Assert.NotNull(order);
            Assert.NotEmpty(cachedOrder);
            _orderRepositoryMock.Verify(x => x.GetCustomerOrdersByIdsAsync(It.IsAny<string[]>(), It.IsAny<string>()), Times.AtMost(3));
        }

        [Fact]
        public async Task GetByIdsAsync_GetThenSaveOrder_ReturnNotCachedOrder()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var fakeOrder = GetFakeCustomerOrder(id);
            var fakeOrderEntity = (CustomerOrderEntity)AbstractTypeFactory<CustomerOrderEntity>.TryCreateInstance().FromModel(fakeOrder, new PrimaryKeyResolvingMap());
            var service = GetCustomerOrderService();
            _cacheEntryMock.SetupGet(c => c.ExpirationTokens).Returns(new List<IChangeToken>());
            var cacheKey = CacheKey.With(service.GetType(), nameof(service.GetByIdsAsync), string.Join("-", new[] { id }), null);
            _platformMemoryCacheMock.Setup(pmc => pmc.CreateEntry(cacheKey)).Returns(_cacheEntryMock.Object);

            //Act
            var nullOrder = await service.GetByIdAsync(id);
            await service.SaveChangesAsync(new[] { fakeOrder });
            _orderRepositoryMock
                .Setup(x => x.GetCustomerOrdersByIdsAsync(new[] { id }, null))
                .ReturnsAsync(new[] { fakeOrderEntity });
            var order = await service.GetByIdAsync(id);
            //need to call again for verify calling times
            await service.GetByIdAsync(id);

            //Assert
            Assert.Null(nullOrder);
            Assert.NotNull(order);
            _orderRepositoryMock.Verify(x => x.GetCustomerOrdersByIdsAsync(It.IsAny<string[]>(), It.IsAny<string>()), Times.AtLeast(4));
        }


        private CustomerOrderService GetCustomerOrderService()
        {
            return GetCustomerOrderService(_platformMemoryCacheMock.Object);
        }

        private CustomerOrderService GetCustomerOrderService(IPlatformMemoryCache platformMemoryCache)
        {
            _shippingMethodsSearchServiceMock
                .Setup(x => x.SearchShippingMethodsAsync(It.IsAny<ShippingMethodsSearchCriteria>()))
                .ReturnsAsync(new ShippingMethodsSearchResult());

            _paymentMethodsSearchServiceMock
                .Setup(x => x.SearchPaymentMethodsAsync(It.IsAny<PaymentMethodsSearchCriteria>()))
                .ReturnsAsync(new PaymentMethodsSearchResult());

            _orderRepositoryMock.Setup(ss => ss.UnitOfWork).Returns(_unitOfWorkMock.Object);
            return new CustomerOrderService(() => _orderRepositoryMock.Object,
                _uniqueNumberGeneratorMock.Object,
                _storeServiceMock.Object,
                _eventPublisherMock.Object,
                _customerOrderTotalsCalculatorMock.Object,
                _shippingMethodsSearchServiceMock.Object,
                _paymentMethodsSearchServiceMock.Object,
                platformMemoryCache);
        }

        private CustomerOrder GetFakeCustomerOrder(string id)
        {
            return new CustomerOrder
            {
                Id = id
            };
        }
    }
}
