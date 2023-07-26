using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Moq;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.OrdersModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.OrdersModule.Tests
{
    [Trait("Category", "CI")]
    public class ZeroPrices
    {
        public static CustomerOrderEntity GetOrderEntity()
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

            order.ResetPrices();
            item.ResetPrices();
            payment.ResetPrices();
            shipment.ResetPrices();

            order.Items = new ObservableCollection<LineItemEntity> { item };
            order.InPayments = new ObservableCollection<PaymentInEntity> { payment };
            order.Shipments = new ObservableCollection<ShipmentEntity> { shipment };

            return order;
        }

        private DefaultCustomerOrderTotalsCalculator GetTotalsCalculator(Currency currency)
        {
            var currencyServiceMock = new Mock<ICurrencyService>();
            currencyServiceMock.Setup(c => c.GetAllCurrenciesAsync()).ReturnsAsync(new List<Currency> { currency });
            return new DefaultCustomerOrderTotalsCalculator(currencyServiceMock.Object);
        }

        [Fact]
        public void CanZeroPrices()
        {
            var order = GetOrderEntity();
            var domainOrder = order.ToModel(AbstractTypeFactory<CustomerOrder>.TryCreateInstance());

            var currency = new Currency
            {
                Code = domainOrder.Currency,
                RoundingPolicy = new DefaultMoneyRoundingPolicy()
            };
            var calc = GetTotalsCalculator(currency);

            calc.CalculateTotals(domainOrder);

            Assert.Equal(0, domainOrder.DiscountAmount);
            Assert.Equal(0, domainOrder.DiscountTotal);
            Assert.Equal(0, domainOrder.DiscountTotalWithTax);
            Assert.Equal(0, domainOrder.Fee);
            Assert.Equal(0, domainOrder.FeeTotal);
            Assert.Equal(0, domainOrder.FeeTotalWithTax);
            Assert.Equal(0, domainOrder.FeeWithTax);
            Assert.Equal(0, domainOrder.HandlingTotal);
            Assert.Equal(0, domainOrder.HandlingTotalWithTax);
            Assert.Equal(0, domainOrder.PaymentDiscountTotal);
            Assert.Equal(0, domainOrder.PaymentDiscountTotalWithTax);
            Assert.Equal(0, domainOrder.PaymentSubTotal);
            Assert.Equal(0, domainOrder.PaymentSubTotalWithTax);
            Assert.Equal(0, domainOrder.PaymentTaxTotal);
            Assert.Equal(0, domainOrder.PaymentTotal);
            Assert.Equal(0, domainOrder.PaymentTotalWithTax);
            Assert.Equal(0, domainOrder.ShippingDiscountTotal);
            Assert.Equal(0, domainOrder.ShippingDiscountTotalWithTax);
            Assert.Equal(0, domainOrder.ShippingSubTotal);
            Assert.Equal(0, domainOrder.ShippingSubTotalWithTax);
            Assert.Equal(0, domainOrder.ShippingTotal);
            Assert.Equal(0, domainOrder.ShippingTotalWithTax);
            Assert.Equal(0, domainOrder.SubTotal);
            Assert.Equal(0, domainOrder.SubTotalDiscount);
            Assert.Equal(0, domainOrder.SubTotalDiscountWithTax);
            Assert.Equal(0, domainOrder.SubTotalTaxTotal);
            Assert.Equal(0, domainOrder.SubTotalWithTax);
            Assert.Equal(0, domainOrder.Sum);
            Assert.Equal(0, domainOrder.TaxPercentRate);
            Assert.Equal(0, domainOrder.TaxTotal);

            Assert.NotEmpty(domainOrder.InPayments);
            var payment = domainOrder.InPayments.First();
            Assert.Equal(0, payment.DiscountAmount);
            Assert.Equal(0, payment.DiscountAmountWithTax);
            Assert.Equal(0, payment.Price);
            Assert.Equal(0, payment.PriceWithTax);
            Assert.Equal(0, payment.Sum);
            Assert.Equal(0, payment.TaxPercentRate);
            Assert.Equal(0, payment.TaxTotal);
            Assert.Equal(0, payment.Total);
            Assert.Equal(0, payment.TotalWithTax);

            Assert.NotEmpty(domainOrder.Shipments);
            var shipment = domainOrder.Shipments.First();
            Assert.Equal(0, shipment.DiscountAmount);
            Assert.Equal(0, shipment.DiscountAmountWithTax);
            Assert.Equal(0, shipment.Fee);
            Assert.Equal(0, shipment.FeeWithTax);
            Assert.Equal(0, shipment.Price);
            Assert.Equal(0, shipment.PriceWithTax);
            Assert.Equal(0, shipment.Sum);
            Assert.Equal(0, shipment.TaxPercentRate);
            Assert.Equal(0, shipment.TaxTotal);
            Assert.Equal(0, shipment.Total);
            Assert.Equal(0, shipment.TotalWithTax);

            Assert.NotEmpty(domainOrder.Items);
            var item = domainOrder.Items.First();
            Assert.Equal(0, item.DiscountAmount);
            Assert.Equal(0, item.DiscountAmountWithTax);
            Assert.Equal(0, item.DiscountTotal);
            Assert.Equal(0, item.DiscountTotalWithTax);
            Assert.Equal(0, item.ExtendedPrice);
            Assert.Equal(0, item.ExtendedPriceWithTax);
            Assert.Equal(0, item.Fee);
            Assert.Equal(0, item.FeeWithTax);
            Assert.Equal(0, item.PlacedPrice);
            Assert.Equal(0, item.PlacedPriceWithTax);
            Assert.Equal(0, item.Price);
            Assert.Equal(0, item.PriceWithTax);
            Assert.Equal(0, item.TaxPercentRate);
            Assert.Equal(0, item.TaxTotal);
        }

        [Fact]
        public void CanZeroPricesWithoutCalc()
        {
            var order = GetOrderEntity();

            Assert.Equal(0, order.TaxPercentRate);
            Assert.Equal(0, order.ShippingTotalWithTax);
            Assert.Equal(0, order.PaymentTotalWithTax);
            Assert.Equal(0, order.DiscountAmount);
            Assert.Equal(0, order.Total);
            Assert.Equal(0, order.SubTotal);
            Assert.Equal(0, order.SubTotalWithTax);
            Assert.Equal(0, order.ShippingTotal);
            Assert.Equal(0, order.PaymentTotal);
            Assert.Equal(0, order.Fee);
            Assert.Equal(0, order.FeeWithTax);
            Assert.Equal(0, order.FeeTotal);
            Assert.Equal(0, order.FeeTotalWithTax);
            Assert.Equal(0, order.HandlingTotal);
            Assert.Equal(0, order.HandlingTotalWithTax);
            Assert.Equal(0, order.DiscountTotal);
            Assert.Equal(0, order.DiscountTotalWithTax);
            Assert.Equal(0, order.TaxTotal);
            Assert.Equal(0, order.Sum);

            Assert.NotEmpty(order.InPayments);
            var payment = order.InPayments.First();
            Assert.Equal(0, payment.Price);
            Assert.Equal(0, payment.PriceWithTax);
            Assert.Equal(0, payment.DiscountAmount);
            Assert.Equal(0, payment.DiscountAmountWithTax);
            Assert.Equal(0, payment.Total);
            Assert.Equal(0, payment.TotalWithTax);
            Assert.Equal(0, payment.TaxTotal);
            Assert.Equal(0, payment.TaxPercentRate);
            Assert.Equal(0, payment.Sum);

            Assert.NotEmpty(order.Shipments);
            var shipment = order.Shipments.First();
            Assert.Equal(0, shipment.Price);
            Assert.Equal(0, shipment.PriceWithTax);
            Assert.Equal(0, shipment.DiscountAmount);
            Assert.Equal(0, shipment.DiscountAmountWithTax);
            Assert.Equal(0, shipment.Total);
            Assert.Equal(0, shipment.TotalWithTax);
            Assert.Equal(0, shipment.TaxTotal);
            Assert.Equal(0, shipment.TaxPercentRate);
            Assert.Equal(0, shipment.Sum);

            Assert.NotEmpty(order.Items);
            var item = order.Items.First();
            Assert.Equal(0, item.Price);
            Assert.Equal(0, item.PriceWithTax);
            Assert.Equal(0, item.DiscountAmount);
            Assert.Equal(0, item.DiscountAmountWithTax);
            Assert.Equal(0, item.TaxTotal);
            Assert.Equal(0, item.TaxPercentRate);
        }
    }
}
