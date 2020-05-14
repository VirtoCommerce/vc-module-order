using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            _cacheEntryMock.SetupGet(c => c.ExpirationTokens).Returns(new List<IChangeToken>());
        }

        [Fact]
        public async Task GetByIdsAsync_GetThenSaveOrder_ReturnCachedOrder()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var newOrder = new CustomerOrder { Id = id };
            var service = GetCustomerOrderServiceWithPlatformMemoryCache();

            //Act
            var nullOrder = await service.GetByIdAsync(id);
            await service.SaveChangesAsync(new[] { newOrder });
            var order = await service.GetByIdAsync(id);

            //Assert
            Assert.NotEqual(nullOrder, order);
        }

        
        private CustomerOrderService GetCustomerOrderServiceWithPlatformMemoryCache()
        {
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);

            var options = new Mock<DbContextOptions<OrderDbContext>>();
            options.Setup(x => x.ContextType).Returns(typeof(DbContext));
            var orderDbConextMock = new Mock<OrderDbContext>(options.Object);
            orderDbConextMock.Setup(x => x.Set<CustomerOrderEntity>())
                .Returns(GetQueryableMockDbSet(new List<CustomerOrderEntity>()));
            var orderRepository = new FakeOrderRepository(orderDbConextMock.Object, _unitOfWorkMock.Object);

            return GetCustomerOrderService(platformMemoryCache, orderRepository);
        }
        
        private CustomerOrderService GetCustomerOrderService()
        {
            return GetCustomerOrderService(_platformMemoryCacheMock.Object, _orderRepositoryMock.Object);
        }

        private CustomerOrderService GetCustomerOrderService(IPlatformMemoryCache platformMemoryCache, IOrderRepository orderRepository)
        {
            _shippingMethodsSearchServiceMock
                .Setup(x => x.SearchShippingMethodsAsync(It.IsAny<ShippingMethodsSearchCriteria>()))
                .ReturnsAsync(new ShippingMethodsSearchResult());

            _paymentMethodsSearchServiceMock
                .Setup(x => x.SearchPaymentMethodsAsync(It.IsAny<PaymentMethodsSearchCriteria>()))
                .ReturnsAsync(new PaymentMethodsSearchResult());

            _orderRepositoryMock.Setup(ss => ss.UnitOfWork).Returns(_unitOfWorkMock.Object);
            return new CustomerOrderService(() => orderRepository,
                _uniqueNumberGeneratorMock.Object,
                _storeServiceMock.Object,
                _eventPublisherMock.Object,
                _customerOrderTotalsCalculatorMock.Object,
                _shippingMethodsSearchServiceMock.Object,
                _paymentMethodsSearchServiceMock.Object,
                platformMemoryCache);
        }

        

        private static DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => sourceList.Add(s));

            return dbSet.Object;
        }
    }


    class FakeOrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _orderDbContext;

        public FakeOrderRepository(OrderDbContext dbContext, IUnitOfWork unitOfWork)
        {
            _orderDbContext = dbContext;
            UnitOfWork = unitOfWork;
        }
        public void Dispose()
        {
            _orderDbContext.Dispose();
        }

        public void Attach<T>(T item) where T : class
        {
            throw new NotImplementedException();
        }

        public void Add<T>(T item) where T : class
        {
            _orderDbContext.Set<T>().Add(item);
        }

        public void Update<T>(T item) where T : class
        {
            throw new NotImplementedException();
        }

        public void Remove<T>(T item) where T : class
        {
            throw new NotImplementedException();
        }

        public IUnitOfWork UnitOfWork { get; }

        public IQueryable<CustomerOrderEntity> CustomerOrders => _orderDbContext.Set<CustomerOrderEntity>();
        public IQueryable<ShipmentEntity> Shipments { get; }
        public IQueryable<PaymentInEntity> InPayments { get; }
        public IQueryable<AddressEntity> Addresses { get; }
        public IQueryable<LineItemEntity> LineItems { get; }
        public Task<CustomerOrderEntity[]> GetCustomerOrdersByIdsAsync(string[] ids, string responseGroup = null)
        {
            return Task.FromResult(CustomerOrders.Where(x => ids.Contains(x.Id)).ToArray());
        }

        public Task<PaymentInEntity[]> GetPaymentsByIdsAsync(string[] ids, string responseGroup = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveOrdersByIdsAsync(string[] ids)
        {
            throw new NotImplementedException();
        }
    }
}
