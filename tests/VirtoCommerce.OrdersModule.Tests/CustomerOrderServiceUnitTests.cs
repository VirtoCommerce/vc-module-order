using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
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
        public async Task GetByIdsAsync_GetThenSaveOrder_ReturnOrder()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var fakeOrder = GetFakeCustomerOrder(id);
            var fakeOrderEntity = (CustomerOrderEntity)AbstractTypeFactory<CustomerOrderEntity>.TryCreateInstance().FromModel(fakeOrder, new PrimaryKeyResolvingMap());
            var service = GetCustomerOrderService();
            var cacheKey = CacheKey.With(service.GetType(), nameof(service.GetByIdsAsync), string.Join("-", id), null);
            _platformMemoryCacheMock.Setup(pmc => pmc.CreateEntry(cacheKey)).Returns(_cacheEntryMock.Object);
            _cacheEntryMock.SetupGet(c => c.ExpirationTokens).Returns(new List<IChangeToken>());

            //Act
            var order = await service.GetByIdAsync(id);
            Assert.Null(order);

            await service.SaveChangesAsync(new[] { fakeOrder });

            _orderRepositoryMock
                .Setup(x => x.GetCustomerOrdersByIdsAsync(new[] {id}, null))
                .ReturnsAsync(new [] { fakeOrderEntity });
            order = await service.GetByIdAsync(id);

            //Assert
            Assert.NotNull(order);
        }


        private CustomerOrderService GetCustomerOrderService()
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
                _platformMemoryCacheMock.Object);
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
