using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
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
    public class OrderTotalPricesTest
    {
        private const decimal taxRate = 0.13M;
        private const decimal discount = 10;

        [Fact]
        public void CustomerOrder_OneItemPerProduct_CheckTotalPrices()
        {
            var order = GetOrder();
            int productQuantity = 1;

            var productsInfo = new List<ProductInfo>
            {
                new ProductInfo("Product 1", 1195M, discount, productQuantity),
                new ProductInfo("Product 2", 159M, discount, productQuantity),
                new ProductInfo("Product 3", 1698M, discount, productQuantity),
                new ProductInfo("Product 4", 3295M, discount, productQuantity),
                new ProductInfo("Product 5", 399M, discount, productQuantity),
                new ProductInfo("Product 6", 1259M, discount, productQuantity),
                new ProductInfo("Product 7", 648M, discount, productQuantity),
                new ProductInfo("Product 8", 110M, discount, productQuantity),
                new ProductInfo("Product 9", 589.99M, discount, productQuantity),
                new ProductInfo("Product 10", 998M, discount, productQuantity),
                new ProductInfo("Product 11", 1199M, discount, productQuantity),
                new ProductInfo("Product 12", 995.99M, discount, productQuantity),
                new ProductInfo("Product 13", 101.19M, discount, productQuantity),
                new ProductInfo("Product 14", 4999M, discount, productQuantity),
                new ProductInfo("Product 15", 223.33M, discount, productQuantity),
                new ProductInfo("Product 16", 4997M, discount, productQuantity),
                new ProductInfo("Product 17", 109.99M, discount, productQuantity),
                new ProductInfo("Product 18", 327.99M, discount, productQuantity),
                new ProductInfo("Product 19", 797.99M, discount, productQuantity),
                new ProductInfo("Product 20", 99M, discount, productQuantity),
                new ProductInfo("Product 21", 399.99M, discount, productQuantity),
                new ProductInfo("Product 22", 109M, discount, productQuantity),
                new ProductInfo("Product 23", 142.75M, discount, productQuantity),
                new ProductInfo("Product 24", 388M, discount, productQuantity),
                new ProductInfo("Product 25", 220.52M, discount, productQuantity),
                new ProductInfo("Product 26", 1498.99M, discount, productQuantity),
                new ProductInfo("Product 27", 999M, discount, productQuantity),
                new ProductInfo("Product 28", 1799.99M, discount, productQuantity),
                new ProductInfo("Product 29", 99.89M, discount, productQuantity),
                new ProductInfo("Product 30", 99.99M, discount, productQuantity),
                new ProductInfo("Product 31", 6999M, discount, productQuantity),
                new ProductInfo("Product 32", 599M, discount, productQuantity),
                new ProductInfo("Product 33", 498M, discount, productQuantity)
            };

            foreach (ProductInfo productInfo in productsInfo)
            {
                LineItem lineItem = GetLineItem(productInfo);
                order.Items.Add(lineItem);
            }

            decimal subtotalRaw = productsInfo.Sum(p => p.Price * p.Quantity);
            decimal discountTotalRaw = productsInfo.Sum(p => (p.Price / discount) * p.Quantity);
            decimal taxesRaw = productsInfo.Sum(p => (p.Price - (p.Price / discount)) * p.Quantity * taxRate);
            decimal totalPriceRaw = subtotalRaw - discountTotalRaw + taxesRaw;

            Assert.Equal(order.SubTotal, subtotalRaw);
            Assert.Equal(order.DiscountTotal, discountTotalRaw);
            Assert.Equal(order.TaxTotal, taxesRaw);
            Assert.Equal(order.Total, totalPriceRaw);
        }

        [Fact]
        public void ShoppingCart_ManyItemsPerProduct_CheckTotalPrices()
        {
            var order = GetOrder();
            int productQuantity = 11;

            var productsInfo = new List<ProductInfo>
            {
                new ProductInfo("Product 1", 109.99M, discount, productQuantity),
                new ProductInfo("Product 2", 101.19M, discount, productQuantity),
                new ProductInfo("Product 3", 99.89M, discount, productQuantity)
            };

            foreach (ProductInfo productInfo in productsInfo)
            {
                LineItem lineItem = GetLineItem(productInfo);
                order.Items.Add(lineItem);
            }

            decimal subtotalRaw = productsInfo.Sum(p => p.Price * p.Quantity);
            decimal discountTotalRaw = productsInfo.Sum(p => (p.Price / discount) * p.Quantity);
            decimal taxesRaw = productsInfo.Sum(p => (p.Price - (p.Price / discount)) * p.Quantity * taxRate);
            decimal totalPriceRaw = subtotalRaw - discountTotalRaw + taxesRaw;

            Assert.Equal(order.SubTotal, subtotalRaw);
            Assert.Equal(order.DiscountTotal, discountTotalRaw);
            Assert.Equal(order.TaxTotal, taxesRaw);
            Assert.Equal(order.Total, totalPriceRaw);
        }

        private CustomerOrder GetOrder() {
            var order = new CustomerOrder
            {
                Items = new List<LineItem>(),
                Discounts = new[] {
                    new Discount
                    {
                        PromotionId = "testPromotion",
                        Currency = "USD",
                        DiscountAmount = discount,
                        Coupon = "ssss"
                    }
                }
            };

            return order;
        }

        private LineItem GetLineItem(ProductInfo productInfo)
        {
            var lineItem = new LineItem
            {
                Name = productInfo.Name,
                Price = productInfo.Price,
                DiscountAmount = productInfo.Price / productInfo.DiscountPercent,
                TaxPercentRate = taxRate,
                Quantity = productInfo.Quantity
            };

            return lineItem;
        }

        private class ProductInfo
        {
            public ProductInfo(string name, decimal price, decimal discountPercent, int quantity)
            {
                Name = name;
                Price = price;
                DiscountPercent = discountPercent;
                Quantity = quantity;
            }

            public string Name { get; set; }

            public decimal Price { get; set; }

            public decimal DiscountPercent { get; set; }

            public int Quantity { get; set; }
        }
    }
}
