using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Order.Model;
using Xunit;

namespace VirtoCommerce.OrderModule.Test
{
    public class OrderTotalPricesTest
    {
        private const decimal taxRate = 0.13M;
        private const decimal discount = 10;

        [Theory]
        [MemberData(nameof(TestProductDataGenerator.GetProducts), MemberType = typeof(TestProductDataGenerator))]
        public void ShoppingCart_CheckTotalPrices(IEnumerable<ProductInfo> productsInfo, decimal subtotalRaw, decimal discountTotalRaw, decimal taxesRaw)
        {
            CustomerOrder order = GetOrder();

            foreach (ProductInfo productInfo in productsInfo)
            {
                LineItem lineItem = GetLineItem(productInfo);
                order.Items.Add(lineItem);
            }
            
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
    }

    public class TestProductDataGenerator
    {
        static decimal discount = 10;
        static decimal taxRate = 0.13M;
        static int singleProductQuantity = 1;
        static int multipleProductQuantity = 11;

        private static IList<ProductInfo> multipleProductsData = new List<ProductInfo>
        {
            new ProductInfo("Product 1", 109.99M, discount, taxRate, multipleProductQuantity),
            new ProductInfo("Product 2", 101.19M, discount, taxRate, multipleProductQuantity),
            new ProductInfo("Product 3", 99.89M, discount, taxRate, multipleProductQuantity)
        };

        private static IList<ProductInfo> singleProductsData = new List<ProductInfo>
        {
            new ProductInfo("Product 1", 1195M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 2", 159M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 3", 1698M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 4", 3295M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 5", 399M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 6", 1259M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 7", 648M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 8", 110M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 9", 589.99M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 10", 998M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 11", 1199M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 12", 995.99M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 13", 101.19M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 14", 4999M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 15", 223.33M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 16", 4997M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 17", 109.99M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 18", 327.99M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 19", 797.99M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 20", 99M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 21", 399.99M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 22", 109M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 23", 142.75M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 24", 388M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 25", 220.52M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 26", 1498.99M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 27", 999M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 28", 1799.99M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 29", 99.89M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 30", 99.99M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 31", 6999M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 32", 599M, discount, taxRate, singleProductQuantity),
            new ProductInfo("Product 33", 498M, discount, taxRate, singleProductQuantity)
        };

        public static IEnumerable<object[]> GetProducts()
        {
            yield return new object[]
            {
                multipleProductsData,
                multipleProductsData.Sum(PriceCalculationPredicates.GetSubTotalValue()),
                multipleProductsData.Sum(PriceCalculationPredicates.GetDiscountTotalValue()),
                multipleProductsData.Sum(PriceCalculationPredicates.GetTaxesValue())
            };

            yield return new object[]
            {
                singleProductsData,
                singleProductsData.Sum(PriceCalculationPredicates.GetSubTotalValue()),
                singleProductsData.Sum(PriceCalculationPredicates.GetDiscountTotalValue()),
                singleProductsData.Sum(PriceCalculationPredicates.GetTaxesValue())
            };
        }
    }

    public static class PriceCalculationPredicates
    {
        public static Func<ProductInfo, decimal> GetSubTotalValue()
        {
            return p => p.Price * p.Quantity;
        }

        public static Func<ProductInfo, decimal> GetDiscountTotalValue()
        {
            return p => (p.Price / p.DiscountPercent) * p.Quantity;
        }

        public static Func<ProductInfo, decimal> GetTaxesValue()
        {
            return p => (p.Price - (p.Price / p.DiscountPercent)) * p.Quantity * p.TaxRate;
        }
    }

    public class ProductInfo
    {
        public ProductInfo(string name, decimal price, decimal discountPercent, decimal taxRate, int quantity)
        {
            Name = name;
            Price = price;
            DiscountPercent = discountPercent;
            TaxRate = taxRate;
            Quantity = quantity;
        }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public decimal DiscountPercent { get; set; }

        public decimal TaxRate { get; set; }

        public int Quantity { get; set; }
    }
}
