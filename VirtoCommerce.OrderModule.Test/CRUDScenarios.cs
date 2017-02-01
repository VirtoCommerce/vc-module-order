using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using VirtoCommerce.CoreModule.Data.Migrations;
using VirtoCommerce.CoreModule.Data.Repositories;
using VirtoCommerce.CoreModule.Data.Services;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Order.Events;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Domain.Payment.Services;
using VirtoCommerce.Domain.Shipping.Services;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.OrderModule.Data.Repositories;
using VirtoCommerce.OrderModule.Data.Services;
using VirtoCommerce.OrderModule.Web.Controllers.Api;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.Platform.Data.Repositories;
using VirtoCommerce.Platform.Testing.Bases;
using Xunit;

namespace VirtoCommerce.OrderModule.Test
{
    // [Trait("Category", "CI")]
    public class CRUDScenarios : FunctionalTestBase
    {
        [Fact]
        public void create_new_order_with_correct_statuses()
        {
            var order = GetTestOrder("order"); // + Guid.NewGuid().ToString()
            var customerOrderService = GetCustomerOrderService();

            Assert.Equal(Domain.Payment.Model.PaymentStatus.Pending, order.InPayments.First().PaymentStatus);
            Assert.Null(order.InPayments.First().Status);

            customerOrderService.SaveChanges(new[] { order });
            order = customerOrderService.GetByIds(new[] { order.Id }).First();


            Assert.NotNull(order);
            Assert.Equal(Domain.Payment.Model.PaymentStatus.Pending, order.InPayments.First().PaymentStatus);
            Assert.NotNull(order.InPayments.First().Status);
        }

        //[Fact]
        //public void Can_update_order_status()
        //{
        //    //arrange
        //    var order = GetTestOrder("order");
        //    var _customerOrderService = GetCustomerOrderService();

        //    //act
        //    order = _customerOrderService.GetByIds(new[] { "order" }).FirstOrDefault();

        //    _customerOrderService.SaveChanges(new[] { order });

        //    ////assert
        //    //Assert.Equal("", GetErrors(payResponse.error));
        //}

        protected CommerceRepositoryImpl GetRepository()
        {
            var repository = new CommerceRepositoryImpl(ConnectionString, new EntityPrimaryKeyGeneratorInterceptor(), new AuditableInterceptor(null));
            EnsureDatabaseInitialized(() => new CommerceRepositoryImpl(ConnectionString), () => Database.SetInitializer(new SetupDatabaseInitializer<CommerceRepositoryImpl, Configuration>()));
            return repository;
        }

        public override void Dispose()
        {
            // Ensure LocalDb databases are deleted after use so that LocalDb doesn't throw if
            // the temp location in which they are stored is later cleaned.
            using (var context = new CommerceRepositoryImpl(ConnectionString))
            {
                context.Database.Delete();
            }
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
                    Coupon = new Coupon
                    {
                        Code = "ssss"
                    }
                }
                }
            };
            var item1 = new LineItem
            {
                Id = "shoes",
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
                    Coupon = new Coupon
                    {
                        Code = "ssss"
                    }
                }}
            };
            var item2 = new LineItem
            {
                Id = "t-shirt",
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
                    Coupon = new Coupon
                    {
                        Code = "ssss"
                    }
                }},

            };

            shipment.Items = new List<ShipmentItem>();
            shipment.Items.AddRange(order.Items.Select(x => new ShipmentItem(x)));

            order.Shipments = new List<Shipment>();
            order.Shipments.Add(shipment);

            var payment = new PaymentIn
            {
                Number = "PI" + id,
                PaymentStatus = Domain.Payment.Model.PaymentStatus.Pending,
                Currency = "USD",
                Sum = 10,
                CustomerId = "et"
            };
            order.InPayments = new List<PaymentIn>();
            order.InPayments.Add(payment);

            return order;
        }

        private static Func<IOrderRepository> GetOrderRepositoryFactory()
        {
            Func<IOrderRepository> orderRepositoryFactory = () =>
            {
                return new OrderRepositoryImpl("VirtoCommerce",
                    new AuditableInterceptor(null),
                    new EntityPrimaryKeyGeneratorInterceptor());
            };
            return orderRepositoryFactory;
        }

        private static CustomerOrderServiceImpl GetCustomerOrderService()
        {
            Func<IPlatformRepository> platformRepositoryFactory = () => new PlatformRepository("VirtoCommerce", new EntityPrimaryKeyGeneratorInterceptor(), new AuditableInterceptor(null));

            //var dynamicPropertyService = new DynamicPropertyService(platformRepositoryFactory);
            var dynamicPropertyService = new Mock<IDynamicPropertyService>().Object;
            var orderEventPublisher = new EventPublisher<OrderChangeEvent>(Enumerable.Empty<IObserver<OrderChangeEvent>>().ToArray());

            var orderService = new CustomerOrderServiceImpl(GetOrderRepositoryFactory(), new TimeBasedNumberGeneratorImpl(), orderEventPublisher, dynamicPropertyService, GetShippingMethodsService(), GetPaymentMethodsService(), GetStoreService(), null);

            return orderService;
        }

        private static OrderModuleController GetCustomerOrderController()
        {
            var orderService = GetCustomerOrderService();

            return new OrderModuleController(orderService, null, null, null, null, null, null, null, null, null, null);
        }

        private static IShippingMethodsService GetShippingMethodsService()
        {
            return new Mock<IShippingMethodsService>().Object;
        }

        private static IPaymentMethodsService GetPaymentMethodsService()
        {
            return new Mock<IPaymentMethodsService>().Object;
        }

        private static IStoreService GetStoreService()
        {
            return new Mock<IStoreService>().Object;
        }


    }
}