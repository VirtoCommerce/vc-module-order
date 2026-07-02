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
        public async Task PlaceCustomerOrderFromCartAsync_LineItemSeam_DispatchesSubtypeByProductType()
        {
            //Arrange - two cart line items differing by the discriminator property
            var physicalCartItem = new LineItem
            {
                Id = Guid.NewGuid().ToString(),
                ProductType = SubtypeDispatchingCustomerOrderBuilder.PhysicalProductType,
                ProductId = "product-physical",
                Sku = "SKU-PHYS",
                ListPrice = 10.99m,
                Quantity = 2,
            };
            var digitalCartItem = new LineItem
            {
                Id = Guid.NewGuid().ToString(),
                ProductType = SubtypeDispatchingCustomerOrderBuilder.DigitalProductType,
                ProductId = "product-digital",
                Sku = "SKU-DIG",
                ListPrice = 4.50m,
                Quantity = 1,
            };
            var cart = new ShoppingCart
            {
                Addresses = new List<Address>(),
                Items = new List<LineItem> { physicalCartItem, digitalCartItem },
            };
            var builder = new SubtypeDispatchingCustomerOrderBuilder(
                new Mock<ICustomerOrderService>().Object,
                new Mock<ISettingsManager>().Object,
                new Mock<IPaymentMethodsSearchService>().Object);

            //Act
            var orderItems = (await builder.PlaceCustomerOrderFromCartAsync(cart)).Items;

            //Assert - each discriminator value produced a distinct order line item subtype, populated on that subtype
            var physicalOrderItem = orderItems.Single(x => x.ProductId == "product-physical");
            var digitalOrderItem = orderItems.Single(x => x.ProductId == "product-digital");

            physicalOrderItem.Should().BeOfType<PhysicalOrderLineItem>();
            physicalOrderItem.Sku.Should().Be("SKU-PHYS");
            physicalOrderItem.Price.Should().Be(10.99m);

            digitalOrderItem.Should().BeOfType<DigitalOrderLineItem>();
            digitalOrderItem.Sku.Should().Be("SKU-DIG");
            digitalOrderItem.Price.Should().Be(4.50m);
        }

        [Fact]
        public async Task PlaceCustomerOrderFromCartAsync_ConfigurationItemSeam_DispatchesSubtypeBySectionName()
        {
            //Arrange - one line item carrying two configuration items differing by section name
            var sectionACartItem = new CartModule.Core.Model.ConfigurationItem
            {
                Id = Guid.NewGuid().ToString(),
                SectionName = SubtypeDispatchingCustomerOrderBuilder.SectionAName,
                ProductId = "config-a",
                Sku = "CFG-A",
                ListPrice = 5.50m,
                Quantity = 1,
                SelectedForCheckout = true,
            };
            var sectionBCartItem = new CartModule.Core.Model.ConfigurationItem
            {
                Id = Guid.NewGuid().ToString(),
                SectionName = SubtypeDispatchingCustomerOrderBuilder.SectionBName,
                ProductId = "config-b",
                Sku = "CFG-B",
                ListPrice = 6.00m,
                Quantity = 1,
                SelectedForCheckout = true,
            };
            var cartItem = new LineItem
            {
                Id = Guid.NewGuid().ToString(),
                ProductId = "product-1",
                Quantity = 1,
                ConfigurationItems = new List<CartModule.Core.Model.ConfigurationItem> { sectionACartItem, sectionBCartItem },
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
            var orderConfigurationItems = orderItem?.ConfigurationItems;

            //Assert - each section name produced a distinct order configuration item subtype, populated on that subtype
            var sectionAOrderItem = orderConfigurationItems.Single(x => x.ProductId == "config-a");
            var sectionBOrderItem = orderConfigurationItems.Single(x => x.ProductId == "config-b");

            sectionAOrderItem.Should().BeOfType<SectionAOrderConfigurationItem>();
            sectionAOrderItem.SectionName.Should().Be(SubtypeDispatchingCustomerOrderBuilder.SectionAName);
            sectionAOrderItem.Sku.Should().Be("CFG-A");
            sectionAOrderItem.Price.Should().Be(5.50m);

            sectionBOrderItem.Should().BeOfType<SectionBOrderConfigurationItem>();
            sectionBOrderItem.SectionName.Should().Be(SubtypeDispatchingCustomerOrderBuilder.SectionBName);
            sectionBOrderItem.Sku.Should().Be("CFG-B");
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
            public const string PhysicalProductType = "physical";
            public const string DigitalProductType = "digital";
            public const string SectionAName = "section-a";
            public const string SectionBName = "section-b";

            public SubtypeDispatchingCustomerOrderBuilder(
                ICustomerOrderService customerOrderService,
                ISettingsManager settingsManager,
                IPaymentMethodsSearchService paymentMethodSearchService)
                : base(customerOrderService, settingsManager, paymentMethodSearchService)
            {
            }

            protected override OrderLineItem CreateOrderModel(CartModule.Core.Model.LineItem lineItem) =>
                lineItem.ProductType switch
                {
                    PhysicalProductType => new PhysicalOrderLineItem(),
                    DigitalProductType => new DigitalOrderLineItem(),
                    _ => base.CreateOrderModel(lineItem),
                };

            protected override OrderConfigurationItem CreateOrderModel(CartModule.Core.Model.ConfigurationItem configurationItem) =>
                configurationItem.SectionName switch
                {
                    SectionAName => new SectionAOrderConfigurationItem(),
                    SectionBName => new SectionBOrderConfigurationItem(),
                    _ => base.CreateOrderModel(configurationItem),
                };
        }

        private sealed class PhysicalOrderLineItem : OrderLineItem
        {
        }

        private sealed class DigitalOrderLineItem : OrderLineItem
        {
        }

        private sealed class SectionAOrderConfigurationItem : OrderConfigurationItem
        {
        }

        private sealed class SectionBOrderConfigurationItem : OrderConfigurationItem
        {
        }
    }
}
