using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VirtoCommerce.CartModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Services;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;
using Address = VirtoCommerce.CartModule.Core.Model.Address;
using LineItem = VirtoCommerce.CartModule.Core.Model.LineItem;

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


        private static CustomerOrderBuilder GetCustomerOrderBuilder()
        {
            return new CustomerOrderBuilder(
                new Mock<ICustomerOrderService>().Object,
                new Mock<ISettingsManager>().Object,
                new Mock<IPaymentMethodsSearchService>().Object,
                new Mock<IItemService>().Object);
        }
    }
}
