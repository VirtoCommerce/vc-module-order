using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.OrdersModule.Data.Services;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Core.Model.Search;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Bus;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.ShippingModule.Core.Model.Search;
using VirtoCommerce.ShippingModule.Core.Services;
using VirtoCommerce.StoreModule.Core.Services;
using Xunit;
using Address = VirtoCommerce.OrdersModule.Core.Model.Address;

namespace VirtoCommerce.OrdersModule.Tests
{
    [Trait("Category", "IntegrationTest")]
    [Trait("Category", "CI")]
    public class CustomerOrderServiceImplIntegrationTests
    {
        private readonly Mock<IStoreService> _storeServiceMock;
        private readonly Mock<IShippingMethodsRegistrar> _shippingMethodRegistrarMock;
        private readonly Mock<IShippingMethodsSearchService> _shippingMethodsSearchServiceMock;
        private readonly Mock<IPaymentMethodsRegistrar> _paymentMethodRegistrarMock;
        private readonly Mock<IPaymentMethodsSearchService> _paymentMethodsSearchService;
        private readonly Mock<ICustomerOrderTotalsCalculator> _customerOrderTotalsCalculatorMock;
        private readonly Mock<IUniqueNumberGenerator> _uniqueNumberGeneratorMock;
        private readonly Mock<IDynamicPropertyService> _dynamicPropertyServiceMock;
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly Mock<IChangeLogService> _changeLogServiceMock;
        private readonly Mock<ICacheEntry> _cacheEntryMock;
        private readonly ICustomerOrderService _customerOrderService;
        private readonly ICustomerOrderSearchService _customerOrderSearchService;
        private readonly Mock<ILogger<PlatformMemoryCache>> _logMock;
        private readonly Mock<ILogger<InProcessBus>> _logEventMock;

        public CustomerOrderServiceImplIntegrationTests()
        {
            _storeServiceMock = new Mock<IStoreService>();
            _shippingMethodRegistrarMock = new Mock<IShippingMethodsRegistrar>();
            _shippingMethodsSearchServiceMock = new Mock<IShippingMethodsSearchService>();
            _shippingMethodsSearchServiceMock.Setup(s => s.SearchShippingMethodsAsync(It.IsAny<ShippingMethodsSearchCriteria>())).ReturnsAsync(new ShippingMethodsSearchResult());
            _paymentMethodRegistrarMock = new Mock<IPaymentMethodsRegistrar>();
            _paymentMethodsSearchService = new Mock<IPaymentMethodsSearchService>();
            _paymentMethodsSearchService.Setup(s => s.SearchPaymentMethodsAsync(It.IsAny<PaymentMethodsSearchCriteria>())).ReturnsAsync(new PaymentMethodsSearchResult());
            _uniqueNumberGeneratorMock = new Mock<IUniqueNumberGenerator>();
            _customerOrderTotalsCalculatorMock = new Mock<ICustomerOrderTotalsCalculator>();
            _dynamicPropertyServiceMock = new Mock<IDynamicPropertyService>();
            _cacheEntryMock = new Mock<ICacheEntry>();
            _cacheEntryMock.SetupGet(c => c.ExpirationTokens).Returns(new List<IChangeToken>());
            _changeLogServiceMock = new Mock<IChangeLogService>();
            _logMock = new Mock<ILogger<PlatformMemoryCache>>();
            _logEventMock = new Mock<ILogger<InProcessBus>>();
            var cachingOptions = new OptionsWrapper<CachingOptions>(new CachingOptions { CacheEnabled = true });
            var memoryCache = new MemoryCache(new MemoryCacheOptions()
            {
                Clock = new SystemClock(),
            });
            _platformMemoryCache = new PlatformMemoryCache(memoryCache, cachingOptions, _logMock.Object);

            var container = new ServiceCollection();
            container.AddDbContext<OrderDbContext>(options => options.UseSqlServer("Data Source=(local);Initial Catalog=VirtoCommerce3_orderTest;Persist Security Info=True;User ID=virto;Password=virto;MultipleActiveResultSets=True;Connect Timeout=30"));
            container.AddScoped<IOrderRepository, OrderRepository>();
            container.AddScoped<ICustomerOrderService, CustomerOrderService>();
            container.AddScoped<ICustomerOrderSearchService, CustomerOrderSearchService>();
            container.AddScoped<Func<IOrderRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetService<IOrderRepository>());
            container.AddScoped<IEventPublisher, InProcessBus>();
            container.AddSingleton(tc => _customerOrderTotalsCalculatorMock.Object);
            container.AddSingleton(x => _uniqueNumberGeneratorMock.Object);
            container.AddSingleton(x => _dynamicPropertyServiceMock.Object);
            container.AddSingleton(x => _storeServiceMock.Object);
            container.AddSingleton(x => _shippingMethodRegistrarMock.Object);
            container.AddSingleton(x => _shippingMethodsSearchServiceMock.Object);
            container.AddSingleton(x => _paymentMethodRegistrarMock.Object);
            container.AddSingleton(x => _paymentMethodsSearchService.Object);
            container.AddSingleton(x => _platformMemoryCache);
            container.AddSingleton(x => _changeLogServiceMock.Object);
            container.AddSingleton(x => _logEventMock.Object);

            var serviceProvider = container.BuildServiceProvider();
            _customerOrderService = serviceProvider.GetService<ICustomerOrderService>();
            _customerOrderSearchService = serviceProvider.GetService<ICustomerOrderSearchService>();
        }

        [Fact]
        public async Task SaveChangesAsync_CreateNewOrder()
        {
            //Arrange
            var order = GetTestOrder($"order{DateTime.Now:O}");

            //Act
            await _customerOrderService.SaveChangesAsync(new[] { order });
            order = await _customerOrderService.GetByIdAsync(order.Id);

            //Assert
            Assert.Equal(PaymentStatus.Pending, order.InPayments.First().PaymentStatus);
            Assert.NotNull(order);
            Assert.Equal(PaymentStatus.Pending, order.InPayments.First().PaymentStatus);
            Assert.NotNull(order.InPayments.First().Status);
        }

        [Fact]
        public async Task SaveChangesAsync_CreateNewOrderAndUpdateOrderState()
        {
            //Arrange
            var orderId = $"order{DateTime.Now:O}";
            var order = GetTestOrder(orderId);
            await _customerOrderService.SaveChangesAsync(new[] { order });

            var criteria = new CustomerOrderSearchCriteria() { Take = 1, Ids = new[] { orderId } };
            var orders = await _customerOrderSearchService.SearchCustomerOrdersAsync(criteria);
            order = orders.Results.FirstOrDefault();

            order.Status = "Authorized";

            //Act
            await _customerOrderService.SaveChangesAsync(new[] { order });


            //Assert
            Assert.NotNull(order);
            Assert.Equal("Authorized", order.Status);
        }

        private static CustomerOrder GetTestOrder(string id)
        {
            var order = new CustomerOrder
            {
                Id = id,
                Number = "CO" + id,
                Currency = "USD",
                CustomerId = "vasja customer",
                EmployeeId = "employe",
                StoreId = "test store",
                Addresses = new[]
                {
                            new Address {
                            AddressType = AddressType.Shipping,
                            City = "london",
                            Phone = "+68787687",
                            PostalCode = "22222",
                            CountryCode = "ENG",
                            CountryName = "England",
                            Email = "user@mail.com",
                            FirstName = "first name",
                            LastName = "last name",
                            Line1 = "line 1",
                            Organization = "org1"
                            }
                        }.ToList(),
                Discounts = new[] {
                    new Discount
                {
                    PromotionId = "testPromotion",
                    Currency = "USD",
                    DiscountAmount = 12,
                    Coupon = "ssss"
                    }
                }
            };
            var item1 = new LineItem
            {
                Id = Guid.NewGuid().ToString(),
                Sku = "shoes",
                Price = 10,
                ProductId = "shoes",
                CatalogId = "catalog",
                Currency = "USD",
                CategoryId = "category",
                Name = "shoes",
                Quantity = 2,
                FulfillmentLocationCode = "warehouse1",
                ShippingMethodCode = "EMS",
                Discounts = new[] {  new Discount
                {
                    PromotionId = "itemPromotion",
                    Currency = "USD",
                    DiscountAmount = 12,
                    Coupon =  "ssss"
                }}
            };
            var item2 = new LineItem
            {
                Id = Guid.NewGuid().ToString(),
                Sku = "t-shirt",
                Price = 100,
                ProductId = "t-shirt",
                CatalogId = "catalog",
                CategoryId = "category",
                Currency = "USD",
                Name = "t-shirt",
                Quantity = 2,
                FulfillmentLocationCode = "warehouse1",
                ShippingMethodCode = "EMS"
            };
            order.Items = new List<LineItem>();
            order.Items.Add(item1);
            order.Items.Add(item2);

            var shipment = new Shipment
            {
                Number = "SH" + id,
                Currency = "USD",
                DeliveryAddress = new Address
                {
                    City = "london",
                    CountryName = "England",
                    Phone = "+68787687",
                    PostalCode = "2222",
                    CountryCode = "ENG",
                    Email = "user@mail.com",
                    FirstName = "first name",
                    LastName = "last name",
                    Line1 = "line 1",
                    Organization = "org1"
                },
                Discounts = new[] {  new Discount
                {
                    PromotionId = "testPromotion",
                    Currency = "USD",
                    DiscountAmount = 12,
                    Coupon = ""
                }},

            };

            shipment.Items = new List<ShipmentItem>();
            shipment.Items.AddRange(order.Items.Select(x => new ShipmentItem(x)));

            order.Shipments = new List<Shipment>();
            order.Shipments.Add(shipment);

            var payment = new PaymentIn
            {
                Number = "PI" + id,
                PaymentStatus = PaymentStatus.Pending,
                Currency = "USD",
                Sum = 10,
                CustomerId = "et"
            };
            order.InPayments = new List<PaymentIn>();
            order.InPayments.Add(payment);

            return order;
        }
    }
}
