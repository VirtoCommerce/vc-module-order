using System.Collections.ObjectModel;
using System.Linq;
using VirtoCommerce.OrderModule.Data.Model;
using Xunit;

namespace VirtoCommerce.OrderModule.Test
{
    public class PatchingTest
    {
        public CustomerOrderEntity PrepareOrder()
        {
            var orderId = "orderId";

            var order = new CustomerOrderEntity
            {
                Id = orderId,
                CustomerId = "customerId",
                StoreId = "storeId",
                Number = "number",
                Currency = "usd",
                PaymentTotal = 1,
                PaymentTotalWithTax = 1,
                ShippingTotalWithTax = 1,
                DiscountAmount = 1,
                DiscountTotal = 1,
                DiscountTotalWithTax = 1,
                HandlingTotal = 1,
                HandlingTotalWithTax = 1,
                ShippingTotal = 1,
                SubTotal = 1,
                SubTotalWithTax = 1,
                Sum = 1,
                TaxPercentRate = 1,
                TaxTotal = 1,
                Total = 1
            };

            var item = new LineItemEntity
            {
                CustomerOrderId = orderId,
                Id = "itemId",
                Currency = "usd",
                ProductId = "productId",
                CatalogId = "catalogId",
                Sku = "sku",
                Name = "itemName",
                Price = 1,
                DiscountAmount = 1,
                TaxPercentRate = 1,
                TaxTotal = 1,
                DiscountAmountWithTax = 1,
                PriceWithTax = 1,
            };

            var payment = new PaymentInEntity
            {
                CustomerOrderId = orderId,
                Id = "paymentId",
                CustomerId = "customerId",
                Number = "number",
                Currency = "usd",
                Price = 1,
                Amount = 1,
                DiscountAmount = 1,
                DiscountAmountWithTax = 1,
                PriceWithTax = 1,
                TaxPercentRate = 1,
                TaxTotal = 1,
                Total = 1,
                Sum = 1,
                TotalWithTax = 1,
            };

            var shipment = new ShipmentEntity
            {
                CustomerOrderId = orderId,
                Id = "shipmentId",
                Number = "number",
                Currency = "usd",
                Price = 1,
                DiscountAmount = 1,
                DiscountAmountWithTax = 1,
                PriceWithTax = 1,
                Sum = 1,
                TaxPercentRate = 1,
                TaxTotal = 1,
                Total = 1,
                TotalWithTax = 1,
            };

            order.Items = new ObservableCollection<LineItemEntity> { item };
            order.InPayments = new ObservableCollection<PaymentInEntity> { payment };
            order.Shipments = new ObservableCollection<ShipmentEntity> { shipment };

            return order;
        }

        [Fact]
        public void DoNotSaveZeroPricesInOrder()
        {
            var oldOrder = PrepareOrder();
            var modifiedOrder = PrepareOrder();

            modifiedOrder.TaxPercentRate = 0m;
            modifiedOrder.ShippingTotalWithTax = 0m;
            modifiedOrder.PaymentTotalWithTax = 0m;
            modifiedOrder.DiscountAmount = 0m;
            modifiedOrder.Total = 2m;
            modifiedOrder.PaymentTotal = 2m;
            modifiedOrder.DiscountTotal = 2m;
            modifiedOrder.DiscountTotalWithTax = 2m;
            modifiedOrder.HandlingTotal = 2m;
            modifiedOrder.HandlingTotalWithTax = 2m;
            modifiedOrder.ShippingTotal = 2m;
            modifiedOrder.SubTotal = 2m;
            modifiedOrder.SubTotalWithTax = 2m;
            modifiedOrder.Sum = 2m;
            modifiedOrder.TaxTotal = 2m;

            modifiedOrder.Patch(oldOrder);

            Assert.Equal(1, oldOrder.TaxPercentRate);
            Assert.Equal(1, oldOrder.ShippingTotalWithTax);
            Assert.Equal(1, oldOrder.PaymentTotalWithTax);
            Assert.Equal(1, oldOrder.DiscountAmount);
            Assert.Equal(1, oldOrder.Total);
            Assert.Equal(1, oldOrder.SubTotal);
            Assert.Equal(1, oldOrder.SubTotalWithTax);
            Assert.Equal(1, oldOrder.ShippingTotal);
            Assert.Equal(1, oldOrder.PaymentTotal);
            Assert.Equal(1, oldOrder.HandlingTotal);
            Assert.Equal(1, oldOrder.HandlingTotalWithTax);
            Assert.Equal(1, oldOrder.DiscountTotal);
            Assert.Equal(1, oldOrder.DiscountTotalWithTax);
            Assert.Equal(1, oldOrder.TaxTotal);
            Assert.Equal(1, oldOrder.Sum);
        }


        [Fact]
        public void CanChangePricesInOrder()
        {
            var oldOrder = PrepareOrder();
            var modifiedOrder = PrepareOrder();

            modifiedOrder.TaxPercentRate = 2m;
            modifiedOrder.ShippingTotalWithTax = 2m;
            modifiedOrder.PaymentTotalWithTax = 2m;
            modifiedOrder.DiscountAmount = 2m;
            modifiedOrder.Total = 2m;
            modifiedOrder.PaymentTotal = 2m;
            modifiedOrder.DiscountTotal = 2m;
            modifiedOrder.DiscountTotalWithTax = 2m;
            modifiedOrder.HandlingTotal = 2m;
            modifiedOrder.HandlingTotalWithTax = 2m;
            modifiedOrder.ShippingTotal = 2m;
            modifiedOrder.SubTotal = 2m;
            modifiedOrder.SubTotalWithTax = 2m;
            modifiedOrder.Sum = 2m;
            modifiedOrder.TaxTotal = 2m;

            modifiedOrder.Patch(oldOrder);

            Assert.Equal(2, oldOrder.TaxPercentRate);
            Assert.Equal(2, oldOrder.ShippingTotalWithTax);
            Assert.Equal(2, oldOrder.PaymentTotalWithTax);
            Assert.Equal(2, oldOrder.DiscountAmount);
            Assert.Equal(2, oldOrder.Total);
            Assert.Equal(2, oldOrder.SubTotal);
            Assert.Equal(2, oldOrder.SubTotalWithTax);
            Assert.Equal(2, oldOrder.ShippingTotal);
            Assert.Equal(2, oldOrder.PaymentTotal);
            Assert.Equal(2, oldOrder.HandlingTotal);
            Assert.Equal(2, oldOrder.HandlingTotalWithTax);
            Assert.Equal(2, oldOrder.DiscountTotal);
            Assert.Equal(2, oldOrder.DiscountTotalWithTax);
            Assert.Equal(2, oldOrder.TaxTotal);
            Assert.Equal(2, oldOrder.Sum);
        }

        [Fact]
        public void CanChangePriceByOneValueInOrder()
        {
            var dataset = new[]
            {
                (0,0,0,2),
                (0,0,2,0),
                (0,2,0,0),
                (2,0,0,0)
            };

            var oldOrder = PrepareOrder();
            var modifiedOrder = PrepareOrder();

            foreach (var data in dataset)
            {
                modifiedOrder.TaxPercentRate = data.Item1;
                modifiedOrder.ShippingTotalWithTax = data.Item2;
                modifiedOrder.PaymentTotalWithTax = data.Item3;
                modifiedOrder.DiscountAmount = data.Item4;

                modifiedOrder.Patch(oldOrder);

                Assert.Equal(data.Item1, oldOrder.TaxPercentRate);
                Assert.Equal(data.Item2, oldOrder.ShippingTotalWithTax);
                Assert.Equal(data.Item3, oldOrder.PaymentTotalWithTax);
                Assert.Equal(data.Item4, oldOrder.DiscountAmount);
            }
        }

        [Fact]
        public void DoNotSaveZeroPricesInItem()
        {
            var oldItem = PrepareOrder().Items.FirstOrDefault();
            var modifiedItem = PrepareOrder().Items.FirstOrDefault();

            modifiedItem.Price = 0;
            modifiedItem.DiscountAmount = 0;
            modifiedItem.TaxPercentRate = 0;
            modifiedItem.TaxTotal = 2;
            modifiedItem.DiscountAmountWithTax = 2;
            modifiedItem.PriceWithTax = 2;

            modifiedItem.Patch(oldItem);

            Assert.Equal(1, oldItem.Price);
            Assert.Equal(1, oldItem.DiscountAmount);
            Assert.Equal(1, oldItem.TaxPercentRate);
            Assert.Equal(1, oldItem.TaxTotal);
            Assert.Equal(1, oldItem.DiscountAmountWithTax);
            Assert.Equal(1, oldItem.PriceWithTax);
        }

        [Fact]
        public void CanChangePricesInItem()
        {
            var oldItem = PrepareOrder().Items.FirstOrDefault();
            var modifiedItem = PrepareOrder().Items.FirstOrDefault();

            modifiedItem.Price = 2;
            modifiedItem.DiscountAmount = 2;
            modifiedItem.TaxPercentRate = 2;
            modifiedItem.TaxTotal = 2;
            modifiedItem.DiscountAmountWithTax = 2;
            modifiedItem.PriceWithTax = 2;

            modifiedItem.Patch(oldItem);

            Assert.Equal(2, oldItem.Price);
            Assert.Equal(2, oldItem.DiscountAmount);
            Assert.Equal(2, oldItem.TaxPercentRate);
            Assert.Equal(2, oldItem.TaxTotal);
            Assert.Equal(2, oldItem.DiscountAmountWithTax);
            Assert.Equal(2, oldItem.PriceWithTax);
        }

        [Fact]
        public void CanChangePriceByOneValueInItem()
        {
            var dataset = new[]
            {
                (0,0,2),
                (0,2,0),
                (2,0,0)
            };

            var oldItem = PrepareOrder().Items.FirstOrDefault();
            var modifiedItem = PrepareOrder().Items.FirstOrDefault();

            foreach (var data in dataset)
            {
                modifiedItem.Price = data.Item2;
                modifiedItem.DiscountAmount = data.Item2;
                modifiedItem.TaxPercentRate = data.Item3;

                modifiedItem.Patch(oldItem);

                Assert.Equal(data.Item2, modifiedItem.Price);
                Assert.Equal(data.Item2, modifiedItem.DiscountAmount);
                Assert.Equal(data.Item3, modifiedItem.TaxPercentRate);
            }
        }

        [Fact]
        public void DoNotSaveZeroPricesInPayment()
        {
            var oldItem = PrepareOrder().InPayments.FirstOrDefault();
            var modifiedItem = PrepareOrder().InPayments.FirstOrDefault();

            modifiedItem.Price = 0;
            modifiedItem.Amount = 0;
            modifiedItem.DiscountAmount = 0;
            modifiedItem.Sum = 0;
            modifiedItem.TaxPercentRate = 0;
            modifiedItem.DiscountAmountWithTax = 2;
            modifiedItem.PriceWithTax = 2;
            modifiedItem.TaxTotal = 2;
            modifiedItem.Total = 2;
            modifiedItem.TotalWithTax = 2;

            modifiedItem.Patch(oldItem);

            Assert.Equal(1, oldItem.Price);
            Assert.Equal(1, oldItem.Amount);
            Assert.Equal(1, oldItem.DiscountAmount);
            Assert.Equal(1, oldItem.Sum);
            Assert.Equal(1, oldItem.TaxPercentRate);
            Assert.Equal(1, oldItem.DiscountAmountWithTax);
            Assert.Equal(1, oldItem.PriceWithTax);
            Assert.Equal(1, oldItem.TaxTotal);
            Assert.Equal(1, oldItem.Total);
            Assert.Equal(1, oldItem.TotalWithTax);
        }

        [Fact]
        public void CanChangePricesInPayments()
        {
            var oldItem = PrepareOrder().InPayments.FirstOrDefault();
            var modifiedItem = PrepareOrder().InPayments.FirstOrDefault();

            modifiedItem.Price = 2;
            modifiedItem.Amount = 2;
            modifiedItem.DiscountAmount = 2;
            modifiedItem.Sum = 2;
            modifiedItem.TaxPercentRate = 2;
            modifiedItem.DiscountAmountWithTax = 2;
            modifiedItem.PriceWithTax = 2;
            modifiedItem.TaxTotal = 2;
            modifiedItem.Total = 2;
            modifiedItem.TotalWithTax = 2;

            modifiedItem.Patch(oldItem);

            Assert.Equal(2, oldItem.Price);
            Assert.Equal(2, oldItem.Amount);
            Assert.Equal(2, oldItem.DiscountAmount);
            Assert.Equal(2, oldItem.Sum);
            Assert.Equal(2, oldItem.TaxPercentRate);
            Assert.Equal(2, oldItem.DiscountAmountWithTax);
            Assert.Equal(2, oldItem.PriceWithTax);
            Assert.Equal(2, oldItem.TaxTotal);
            Assert.Equal(2, oldItem.Total);
            Assert.Equal(2, oldItem.TotalWithTax);
        }

        [Fact]
        public void CanChangePricesByOneValueInPayments()
        {
            var dataset = new[]
            {
                (0,0,0,0,2),
                (0,0,0,2,0),
                (0,0,2,0,0),
                (0,2,0,0,0),
                (2,0,0,0,0)
            };

            var oldItem = PrepareOrder().InPayments.FirstOrDefault();
            var modifiedItem = PrepareOrder().InPayments.FirstOrDefault();

            foreach (var data in dataset)
            {
                modifiedItem.Price = data.Item1;
                modifiedItem.Amount = data.Item2;
                modifiedItem.DiscountAmount = data.Item3;
                modifiedItem.Sum = data.Item4;
                modifiedItem.TaxPercentRate = data.Item5;

                modifiedItem.Patch(oldItem);

                Assert.Equal(data.Item1, oldItem.Price);
                Assert.Equal(data.Item2, oldItem.Amount);
                Assert.Equal(data.Item3, oldItem.DiscountAmount);
                Assert.Equal(data.Item4, oldItem.Sum);
                Assert.Equal(data.Item5, oldItem.TaxPercentRate);
            }
        }

        [Fact]
        public void DoNotSaveZeroPricesInShipments()
        {
            var oldItem = PrepareOrder().Shipments.FirstOrDefault();
            var modifiedItem = PrepareOrder().Shipments.FirstOrDefault();

            modifiedItem.Price = 0;
            modifiedItem.DiscountAmount = 0;
            modifiedItem.TaxPercentRate = 0;
            modifiedItem.DiscountAmountWithTax = 2;
            modifiedItem.PriceWithTax = 2;
            modifiedItem.Sum = 2;
            modifiedItem.TaxPercentRate = 0;
            modifiedItem.TaxTotal = 2;
            modifiedItem.Total = 2;
            modifiedItem.TotalWithTax = 2;

            modifiedItem.Patch(oldItem);

            Assert.Equal(1, oldItem.Price);
            Assert.Equal(1, oldItem.DiscountAmount);
            Assert.Equal(1, oldItem.TaxPercentRate);
            Assert.Equal(1, oldItem.DiscountAmountWithTax);
            Assert.Equal(1, oldItem.PriceWithTax);
            Assert.Equal(1, oldItem.Sum);
            Assert.Equal(1, oldItem.TaxPercentRate);
            Assert.Equal(1, oldItem.TaxTotal);
            Assert.Equal(1, oldItem.Total);
            Assert.Equal(1, oldItem.TotalWithTax);
        }

        [Fact]
        public void CanChangePricesInShipment()
        {
            var oldItem = PrepareOrder().Shipments.FirstOrDefault();
            var modifiedItem = PrepareOrder().Shipments.FirstOrDefault();

            modifiedItem.Price = 2;
            modifiedItem.DiscountAmount = 2;
            modifiedItem.TaxPercentRate = 2;
            modifiedItem.DiscountAmountWithTax = 2;
            modifiedItem.PriceWithTax = 2;
            modifiedItem.Sum = 2;
            modifiedItem.TaxTotal = 2;
            modifiedItem.Total = 2;
            modifiedItem.TotalWithTax = 2;

            modifiedItem.Patch(oldItem);

            Assert.Equal(2, oldItem.Price);
            Assert.Equal(2, oldItem.DiscountAmount);
            Assert.Equal(2, oldItem.TaxPercentRate);
            Assert.Equal(2, oldItem.DiscountAmountWithTax);
            Assert.Equal(2, oldItem.PriceWithTax);
            Assert.Equal(2, oldItem.Sum);
            Assert.Equal(2, oldItem.TaxTotal);
            Assert.Equal(2, oldItem.Total);
            Assert.Equal(2, oldItem.TotalWithTax);
        }

        [Fact]
        public void CanChangePricesByOneValueInShipment()
        {
            var dataset = new[]
            {
                (0,0,2),
                (0,2,0),
                (2,0,0)
            };

            var oldItem = PrepareOrder().InPayments.FirstOrDefault();
            var modifiedItem = PrepareOrder().InPayments.FirstOrDefault();

            foreach (var data in dataset)
            {
                modifiedItem.Price = data.Item1;
                modifiedItem.DiscountAmount = data.Item2;
                modifiedItem.TaxPercentRate = data.Item3;

                modifiedItem.Patch(oldItem);

                Assert.Equal(data.Item1, oldItem.Price);
                Assert.Equal(data.Item2, oldItem.DiscountAmount);
                Assert.Equal(data.Item3, oldItem.TaxPercentRate);
            }
        }
    }
}
