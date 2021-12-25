using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.OrdersModule.Tests
{
    public class CustomerOrderBuilderUnitTests
    {
        private readonly Mock<ICustomerOrderService> _customerOrderServiceMock;

        public CustomerOrderBuilderUnitTests()
        {
            _customerOrderServiceMock = new Mock<ICustomerOrderService>();
        }

        

        [Fact]
        public async Task PlaceCustomerOrderFromCartAsync_Cart_ComparingWithOrder()
        {
            //Arrange
            var cartItem1 = new VirtoCommerce.CartModule.Core.Model.LineItem { Id = Guid.NewGuid().ToString(), ListPrice = 10.99m, SalePrice = 9.66m, DiscountAmount = 1.33m, TaxPercentRate = 0.12m, Quantity = 2 };
            var cart = new VirtoCommerce.CartModule.Core.Model.ShoppingCart
            {
                Addresses = new List<CartModule.Core.Model.Address>(),
                Items = new List<VirtoCommerce.CartModule.Core.Model.LineItem> { cartItem1 }
            };
            var builder = GetCustomerOrderBuilder();

            //Act
            var result = (await builder.PlaceCustomerOrderFromCartAsync(cart));

            //Assert
            cart.Should()
                .BeEquivalentTo(result
                    , options => options
                                 .ComparingByMembers<VirtoCommerce.OrdersModule.Core.Model.CustomerOrder>()
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
                                 .ComparingByMembers<VirtoCommerce.OrdersModule.Core.Model.LineItem>()
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
            var cartItem1 = new VirtoCommerce.CartModule.Core.Model.LineItem
            {
                Id = Guid.NewGuid().ToString(),
                ListPrice = 10.99m,
                SalePrice = 9.66m,
                DiscountAmount = 1.33m,
                TaxPercentRate = 0.12m,
                Fee = 0.33m,
                Quantity = 2
            };

            var cart = new VirtoCommerce.CartModule.Core.Model.ShoppingCart
            {
                Addresses = new List<CartModule.Core.Model.Address>(),
                Items = new List<VirtoCommerce.CartModule.Core.Model.LineItem> { cartItem1 }
            };
            var builder = GetCustomerOrderBuilder();

            //Act
            var orderItem = (await builder.PlaceCustomerOrderFromCartAsync(cart)).Items.FirstOrDefault();

            //Assert
            cartItem1.Should()
                .BeEquivalentTo(orderItem
                    , options => options
                                 .ComparingByMembers<VirtoCommerce.OrdersModule.Core.Model.LineItem>()
                                 .ExcludingMissingMembers()
                                 .Excluding(x => x.Id)
                                 .Excluding(x => x.ObjectType)
                                 .Excluding(x => x.Price)
                                 .Excluding(x => x.Fee)
                                 );
            Assert.Equal(cartItem1.ListPrice, orderItem.Price);
            Assert.Equal(0.33m, orderItem.Fee);
        }


        private CustomerOrderBuilder GetCustomerOrderBuilder()
        {
            return new CustomerOrderBuilder(_customerOrderServiceMock.Object);
        }
    }
}
