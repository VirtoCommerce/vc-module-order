using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VirtoCommerce.CartModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Services;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;
using Address = VirtoCommerce.CartModule.Core.Model.Address;
using LineItem = VirtoCommerce.CartModule.Core.Model.LineItem;
using OrderConfigurationItem = VirtoCommerce.OrdersModule.Core.Model.ConfigurationItem;
using OrderLineItem = VirtoCommerce.OrdersModule.Core.Model.LineItem;

namespace VirtoCommerce.OrdersModule.Tests
{
    public class CustomerOrderBuilderUnitTests
    {
        [Fact]
        public async Task PlaceCustomerOrderFromCartAsync_Cart_ComparingWithOrder()
        {
            //Arrange
            var cartItem1 = new LineItem { Id = Guid.NewGuid().ToString(), ListPrice = 10.99m, SalePrice = 9.66m, DiscountAmount = 1.33m, TaxPercentRate = 0.12m, Quantity = 2 };
            var cart = new ShoppingCart
            {
                Addresses = new List<Address>(),
                Items = new List<LineItem> { cartItem1 }
            };
            var builder = GetCustomerOrderBuilder();

            //Act
            var result = (await builder.PlaceCustomerOrderFromCartAsync(cart));

            //Assert
            cart.Should()
                .BeEquivalentTo(result
                    , options => options
                                 .ComparingByMembers<CustomerOrder>()
                                 //.RespectingDeclaredTypes()
                                 .ExcludingMissingMembers()
                                 .Excluding(x => x.Id)
                                 .Excluding(x => x.ObjectType)
                                 .Excluding(x => x.Status)
                                 .Excluding(x => x.Items)
                                 );

            var orderItem = result.Items.FirstOrDefault();
            cartItem1.Should()
                .BeEquivalentTo(orderItem
                    , options => options
                                 .ComparingByMembers<Core.Model.LineItem>()
                                 .ExcludingMissingMembers()
                                 .Excluding(x => x.Id)
                                 .Excluding(x => x.ObjectType)
                                 .Excluding(x => x.Price)
                                 );
            Assert.Equal(cartItem1.ListPrice, orderItem.Price);
        }

        [Fact]
        public async Task PlaceCustomerOrderFromCartAsync_CartLineItem_ComparingWithOrderLineItem()
        {
            //Arrange
            var cartItem1 = new LineItem
            {
                Id = Guid.NewGuid().ToString(),
                ListPrice = 10.99m,
                SalePrice = 9.66m,
                DiscountAmount = 1.33m,
                TaxPercentRate = 0.12m,
                Fee = 0.33m,
                FeeWithTax = 0.34m,
                Quantity = 2
            };

            var cart = new ShoppingCart
            {
                Addresses = new List<Address>(),
                Items = new List<LineItem> { cartItem1 },
            };
            var builder = GetCustomerOrderBuilder();

            //Act
            var orderItem = (await builder.PlaceCustomerOrderFromCartAsync(cart)).Items.FirstOrDefault();

            //Assert
            cartItem1.Should()
                .BeEquivalentTo(orderItem
                    , options => options
                                 .ComparingByMembers<Core.Model.LineItem>()
                                 .ExcludingMissingMembers()
                                 .Excluding(x => x.Id)
                                 .Excluding(x => x.ObjectType)
                                 .Excluding(x => x.Price)
                                 .Excluding(x => x.Fee)
                                 );
            Assert.Equal(cartItem1.ListPrice, orderItem.Price);
            Assert.Equal(0.33m, orderItem.Fee);
        }


        [Fact]
        public void CreateOrderModel_LineItem_Default_ReturnsFactoryInstance()
        {
            //Arrange
            var builder = new TestableCustomerOrderBuilder(
                new Mock<ICustomerOrderService>().Object,
                new Mock<ISettingsManager>().Object,
                new Mock<IPaymentMethodsSearchService>().Object);
            var cartItem = new LineItem { Id = Guid.NewGuid().ToString() };

            //Act
            var result = builder.CreateOrderModelPublic(cartItem);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OrderLineItem>();
        }

        [Fact]
        public void CreateOrderModel_ConfigurationItem_Default_ReturnsFactoryInstance()
        {
            //Arrange
            var builder = new TestableCustomerOrderBuilder(
                new Mock<ICustomerOrderService>().Object,
                new Mock<ISettingsManager>().Object,
                new Mock<IPaymentMethodsSearchService>().Object);
            var cartConfigurationItem = new CartModule.Core.Model.ConfigurationItem { Id = Guid.NewGuid().ToString() };

            //Act
            var result = builder.CreateOrderModelPublic(cartConfigurationItem);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OrderConfigurationItem>();
        }

        [Fact]
        public async Task PlaceCustomerOrderFromCartAsync_OverriddenLineItemSeam_PopulationRunsOnSubtype()
        {
            //Arrange
            var cartItem = new LineItem
            {
                Id = Guid.NewGuid().ToString(),
                ProductId = "product-1",
                Sku = "SKU-1",
                ListPrice = 10.99m,
                Quantity = 2,
            };
            var cart = new ShoppingCart
            {
                Addresses = new List<Address>(),
                Items = new List<LineItem> { cartItem },
            };
            var builder = new SubtypeDispatchingCustomerOrderBuilder(
                new Mock<ICustomerOrderService>().Object,
                new Mock<ISettingsManager>().Object,
                new Mock<IPaymentMethodsSearchService>().Object);

            //Act
            var orderItem = (await builder.PlaceCustomerOrderFromCartAsync(cart)).Items.FirstOrDefault();

            //Assert
            orderItem.Should().BeOfType<TestOrderLineItem>();
            orderItem.ProductId.Should().Be("product-1");
            orderItem.Sku.Should().Be("SKU-1");
            orderItem.Price.Should().Be(10.99m);
        }

        [Fact]
        public async Task PlaceCustomerOrderFromCartAsync_OverriddenConfigurationItemSeam_PopulationRunsOnSubtype()
        {
            //Arrange
            var cartConfigurationItem = new CartModule.Core.Model.ConfigurationItem
            {
                Id = Guid.NewGuid().ToString(),
                ProductId = "config-product-1",
                Sku = "CFG-SKU-1",
                ListPrice = 5.50m,
                Quantity = 1,
                SelectedForCheckout = true,
            };
            var cartItem = new LineItem
            {
                Id = Guid.NewGuid().ToString(),
                ProductId = "product-1",
                Quantity = 1,
                ConfigurationItems = new List<CartModule.Core.Model.ConfigurationItem> { cartConfigurationItem },
            };
            var cart = new ShoppingCart
            {
                Addresses = new List<Address>(),
                Items = new List<LineItem> { cartItem },
            };
            var builder = new SubtypeDispatchingCustomerOrderBuilder(
                new Mock<ICustomerOrderService>().Object,
                new Mock<ISettingsManager>().Object,
                new Mock<IPaymentMethodsSearchService>().Object);

            //Act
            var orderItem = (await builder.PlaceCustomerOrderFromCartAsync(cart)).Items.FirstOrDefault();
            var orderConfigurationItem = orderItem?.ConfigurationItems.FirstOrDefault();

            //Assert
            orderConfigurationItem.Should().BeOfType<TestOrderConfigurationItem>();
            orderConfigurationItem.ProductId.Should().Be("config-product-1");
            orderConfigurationItem.Sku.Should().Be("CFG-SKU-1");
            orderConfigurationItem.Price.Should().Be(5.50m);
        }

        private static CustomerOrderBuilder GetCustomerOrderBuilder()
        {
            return new CustomerOrderBuilder(
                new Mock<ICustomerOrderService>().Object,
                new Mock<ISettingsManager>().Object,
                new Mock<IPaymentMethodsSearchService>().Object);
        }

        private sealed class TestableCustomerOrderBuilder : CustomerOrderBuilder
        {
            public TestableCustomerOrderBuilder(
                ICustomerOrderService customerOrderService,
                ISettingsManager settingsManager,
                IPaymentMethodsSearchService paymentMethodSearchService)
                : base(customerOrderService, settingsManager, paymentMethodSearchService)
            {
            }

            public OrderLineItem CreateOrderModelPublic(CartModule.Core.Model.LineItem lineItem) => CreateOrderModel(lineItem);

            public OrderConfigurationItem CreateOrderModelPublic(CartModule.Core.Model.ConfigurationItem configurationItem) => CreateOrderModel(configurationItem);
        }

        private sealed class SubtypeDispatchingCustomerOrderBuilder : CustomerOrderBuilder
        {
            public SubtypeDispatchingCustomerOrderBuilder(
                ICustomerOrderService customerOrderService,
                ISettingsManager settingsManager,
                IPaymentMethodsSearchService paymentMethodSearchService)
                : base(customerOrderService, settingsManager, paymentMethodSearchService)
            {
            }

            protected override OrderLineItem CreateOrderModel(CartModule.Core.Model.LineItem lineItem) => new TestOrderLineItem();

            protected override OrderConfigurationItem CreateOrderModel(CartModule.Core.Model.ConfigurationItem configurationItem) => new TestOrderConfigurationItem();
        }

        private sealed class TestOrderLineItem : OrderLineItem
        {
        }

        private sealed class TestOrderConfigurationItem : OrderConfigurationItem
        {
        }
    }
}
