using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CartModule.Core.Model;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Tax;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using Address = VirtoCommerce.OrdersModule.Core.Model.Address;
using LineItem = VirtoCommerce.OrdersModule.Core.Model.LineItem;
using Shipment = VirtoCommerce.OrdersModule.Core.Model.Shipment;
using ShipmentItem = VirtoCommerce.OrdersModule.Core.Model.ShipmentItem;

namespace VirtoCommerce.OrdersModule.Data.Services
{
    public class CustomerOrderBuilder : ICustomerOrderBuilder
    {
        private readonly ICustomerOrderService _customerOrderService;

        public CustomerOrderBuilder(ICustomerOrderService customerOrderService)
        {
            _customerOrderService = customerOrderService;
        }

        #region ICustomerOrderConverter Members

        public virtual async Task<CustomerOrder> PlaceCustomerOrderFromCartAsync(ShoppingCart cart)
        {
            var customerOrder = ConvertCartToOrder(cart);
            await _customerOrderService.SaveChangesAsync(new[] { customerOrder });
            return customerOrder;
        }

        #endregion
        protected virtual CustomerOrder ConvertCartToOrder(ShoppingCart cart)
        {
            var cartLineItemsMap = new Dictionary<string, LineItem>();

            // Copy Native Properties
            var order = ToOrderModel(cart);

            // Copy LineItems
            if (cart.Items != null)
            {
                order.Items = ToOrderModel(cart.Items, cartLineItemsMap);
            }

            // Copy Discounts
            if (cart.Discounts != null)
            {
                order.Discounts = ToOrderModel(cart.Discounts);
            }

            // Copy Shipments
            if (cart.Shipments != null)
            {
                order.Shipments = ToOrderModel(cart.Shipments, cartLineItemsMap);
            }

            // Copy Payments
            if (cart.Payments != null)
            {
                order.InPayments = ToOrderModel(cart, cart.Payments);
            }

            // Copy DynamicProperties
            if (cart.DynamicProperties != null)
            {
                order.DynamicProperties = ToOrderModel(cart.DynamicProperties);
            }

            // Copy Addresses
            if (cart.Addresses != null)
            {
                order.Addresses = ToOrderModel(cart.Addresses);
            }
            else
            {
                order.Addresses = new List<Address>();
            }

            CopyOtherAddress(cart, order);

            //Save only distinct addresses for order
            if (order.Addresses != null)
            {
                order.Addresses = DistinctAddresses(order.Addresses);
            }

            // Copy TaxDetails
            if (cart.TaxDetails != null)
            {
                order.TaxDetails = ToOrderModel(cart.TaxDetails);
            }

            PostConvertCartToOrder(cart, order, cartLineItemsMap);

            return order;
        }

        protected virtual void PostConvertCartToOrder(ShoppingCart cart, CustomerOrder order, Dictionary<string, LineItem> cartLineItemsMap)
        {
        }

        protected virtual void CopyOtherAddress(ShoppingCart cart, CustomerOrder order)
        {
            //Add shipping address to order
            if (order.Shipments != null)
            {
                order.Addresses.AddRange(order.Shipments.Where(x => x.DeliveryAddress != null).Select(x => x.DeliveryAddress));
            }

            //Add payment address to order
            if (cart.Payments != null)
            {
                order.Addresses.AddRange(cart.Payments.Where(x => x.BillingAddress != null).Select(x => ToOrderModel(x.BillingAddress)));
            }
        }

        protected virtual ICollection<Address> DistinctAddresses(ICollection<Address> addresses)
        {
            var retVal = addresses.Distinct().ToList();
            foreach (var address in retVal)
            {
                //Reset primary key for addresses
                address.Key = null;
            }
            return retVal;
        }

        protected virtual ICollection<TaxDetail> ToOrderModel(ICollection<TaxDetail> cartTaxDetails)
        {
            return cartTaxDetails;
        }

        protected virtual IList<DynamicObjectProperty> ToOrderModel(ICollection<DynamicObjectProperty> cartDynamicProperties)
        {
           return cartDynamicProperties.Select(ToOrderModel).ToList();
        }

        protected virtual List<PaymentIn> ToOrderModel(ShoppingCart cart, ICollection<Payment> cartPayments)
        {
            var retVal = new List<PaymentIn>();
            foreach (var payment in cartPayments)
            {
                var paymentIn = ToOrderModel(payment);
                paymentIn.CustomerId = cart.CustomerId;
                retVal.Add(paymentIn);
            }
            return retVal;
        }

        protected virtual List<Shipment> ToOrderModel(ICollection<VirtoCommerce.CartModule.Core.Model.Shipment> cartShipments, Dictionary<string, LineItem> cartLineItemsMap)
        {
            var retVal = new List<Shipment>();

            foreach (var cartShipment in cartShipments)
            {
                var shipment = ToOrderModel(cartShipment);

                if (!cartShipment.Items.IsNullOrEmpty())
                {
                    shipment.Items = new List<ShipmentItem>();
                    foreach (var cartShipmentItem in cartShipment.Items)
                    {
                        var shipmentItem = ToOrderModel(cartShipmentItem);
                        if (cartLineItemsMap.ContainsKey(cartShipmentItem.LineItemId))
                        {
                            shipmentItem.LineItem = cartLineItemsMap[cartShipmentItem.LineItemId];
                            shipment.Items.Add(shipmentItem);
                        }
                    }
                }

                retVal.Add(shipment);
            }

            return retVal;
        }

        protected virtual List<Address> ToOrderModel(ICollection<VirtoCommerce.CartModule.Core.Model.Address> cartAddress)
        {
            return cartAddress.Select(ToOrderModel).ToList();
        }

        protected virtual List<Discount> ToOrderModel(ICollection<Discount> cartDiscounts)
        {
            return cartDiscounts.Select(ToOrderModel).ToList();
        }

        protected virtual List<LineItem> ToOrderModel(ICollection<CartModule.Core.Model.LineItem> cartLineItems, Dictionary<string, LineItem> cartLineItemsMap)
        {
            var retVal = new List<LineItem>();
            foreach (var cartLineItem in cartLineItems.Where(x => !x.IsRejected))
            {
                var orderLineItem = ToOrderModel(cartLineItem);
                retVal.Add(orderLineItem);
                cartLineItemsMap.Add(cartLineItem.Id, orderLineItem);
            }
            return retVal;
        }

        protected virtual CustomerOrder ToOrderModel(ShoppingCart cart)
        {
            var order = AbstractTypeFactory<CustomerOrder>.TryCreateInstance();

            order.ShoppingCartId = cart.Id;
            order.PurchaseOrderNumber = cart.PurchaseOrderNumber;
            order.Comment = cart.Comment;
            order.Currency = cart.Currency;
            order.ChannelId = cart.ChannelId;
            order.CustomerId = cart.CustomerId;
            order.CustomerName = cart.CustomerName;
            order.DiscountAmount = cart.DiscountAmount;
            order.OrganizationId = cart.OrganizationId;
            order.StoreId = cart.StoreId;
            order.TaxPercentRate = cart.TaxPercentRate;
            order.TaxType = cart.TaxType;
            order.LanguageCode = cart.LanguageCode;
            order.Fee = cart.Fee;
            order.FeeWithTax = cart.FeeWithTax;
            order.FeeTotal = cart.FeeTotal;
            order.FeeTotalWithTax = cart.FeeTotalWithTax;
            order.HandlingTotal = cart.HandlingTotal;
            order.HandlingTotalWithTax = cart.HandlingTotalWithTax;

            order.Status = GetDefaultOrderStatus();

            return order;
        }

        protected virtual string GetDefaultOrderStatus()
        {
            return "New";
        }

        protected virtual LineItem ToOrderModel(CartModule.Core.Model.LineItem lineItem)
        {
            if (lineItem == null)
                throw new ArgumentNullException(nameof(lineItem));

            var retVal = AbstractTypeFactory<LineItem>.TryCreateInstance();

            retVal.CatalogId = lineItem.CatalogId;
            retVal.CategoryId = lineItem.CategoryId;
            retVal.Comment = lineItem.Note;
            retVal.Currency = lineItem.Currency;
            retVal.Height = lineItem.Height;
            retVal.ImageUrl = lineItem.ImageUrl;
            retVal.IsGift = lineItem.IsGift;
            retVal.Length = lineItem.Length;
            retVal.MeasureUnit = lineItem.MeasureUnit;
            retVal.Name = lineItem.Name;
            retVal.PriceId = lineItem.PriceId;
            retVal.ProductId = lineItem.ProductId;
            retVal.ProductType = lineItem.ProductType;
            retVal.Quantity = lineItem.Quantity;
            retVal.Sku = lineItem.Sku;
            retVal.TaxPercentRate = lineItem.TaxPercentRate;
            retVal.TaxType = lineItem.TaxType;
            retVal.Weight = lineItem.Weight;
            retVal.WeightUnit = lineItem.WeightUnit;
            retVal.Width = lineItem.Width;
            retVal.FulfillmentCenterId = lineItem.FulfillmentCenterId;
            retVal.FulfillmentCenterName = lineItem.FulfillmentCenterName;

            retVal.Fee = lineItem.Fee;
            retVal.FeeWithTax = lineItem.FeeWithTax;

            retVal.DiscountAmount = lineItem.DiscountAmount;
            retVal.Price = lineItem.ListPrice;

            retVal.FulfillmentLocationCode = lineItem.FulfillmentLocationCode;

            if (lineItem.DynamicProperties != null)
            {
                retVal.DynamicProperties = lineItem.DynamicProperties.Select(ToOrderModel).ToList();
            }

            if (lineItem.Discounts != null)
            {
                retVal.Discounts = lineItem.Discounts.Select(ToOrderModel).ToList();
            }
            retVal.TaxDetails = lineItem.TaxDetails;
            return retVal;
        }

        protected virtual Discount ToOrderModel(Discount discount)
        {
            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            var retVal = AbstractTypeFactory<Discount>.TryCreateInstance();

            retVal.Coupon = discount.Coupon;
            retVal.Currency = discount.Currency;
            retVal.Description = discount.Description;
            retVal.DiscountAmount = discount.DiscountAmount;
            retVal.DiscountAmountWithTax = discount.DiscountAmountWithTax;
            retVal.PromotionId = discount.PromotionId;

            return retVal;
        }

        protected virtual Shipment ToOrderModel(CartModule.Core.Model.Shipment shipment)
        {
            var retVal = AbstractTypeFactory<Shipment>.TryCreateInstance();

            retVal.Currency = shipment.Currency;
            retVal.DiscountAmount = shipment.DiscountAmount;
            retVal.Height = shipment.Height;
            retVal.Length = shipment.Length;
            retVal.MeasureUnit = shipment.MeasureUnit;
            retVal.FulfillmentCenterId = shipment.FulfillmentCenterId;
            retVal.FulfillmentCenterName = shipment.FulfillmentCenterName;
            retVal.ShipmentMethodCode = shipment.ShipmentMethodCode;
            retVal.ShipmentMethodOption = shipment.ShipmentMethodOption;
            retVal.Sum = shipment.Total;
            retVal.Weight = shipment.Weight;
            retVal.WeightUnit = shipment.WeightUnit;
            retVal.Width = shipment.Width;
            retVal.TaxPercentRate = shipment.TaxPercentRate;
            retVal.DiscountAmount = shipment.DiscountAmount;
            retVal.Price = shipment.Price;
            retVal.Fee = shipment.Fee;
            retVal.Fee = shipment.FeeWithTax;
            retVal.Status = "New";
            if (shipment.DeliveryAddress != null)
            {
                retVal.DeliveryAddress = ToOrderModel(shipment.DeliveryAddress);
                retVal.DeliveryAddress.Key = null;
            }
            if (shipment.Discounts != null)
            {
                retVal.Discounts = shipment.Discounts.Select(ToOrderModel).ToList();
            }

            if (shipment.DynamicProperties != null)
            {
                retVal.DynamicProperties = shipment.DynamicProperties.Select(ToOrderModel).ToList();
            }

            retVal.TaxDetails = shipment.TaxDetails;
            return retVal;
        }

        protected virtual ShipmentItem ToOrderModel(CartModule.Core.Model.ShipmentItem shipmentItem)
        {
            if (shipmentItem == null)
                throw new ArgumentNullException(nameof(shipmentItem));

            var retVal = AbstractTypeFactory<ShipmentItem>.TryCreateInstance();
            retVal.BarCode = shipmentItem.BarCode;
            retVal.Quantity = shipmentItem.Quantity;
            return retVal;
        }

        protected virtual PaymentIn ToOrderModel(Payment payment)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            var retVal = AbstractTypeFactory<PaymentIn>.TryCreateInstance();
            retVal.Currency = payment.Currency;
            retVal.DiscountAmount = payment.DiscountAmount;
            retVal.Price = payment.Price;
            retVal.TaxPercentRate = payment.TaxPercentRate;
            retVal.TaxType = payment.TaxType;

            retVal.GatewayCode = payment.PaymentGatewayCode;
            retVal.Sum = payment.Amount;
            retVal.PaymentStatus = PaymentStatus.New;
            if (payment.BillingAddress != null)
            {
                retVal.BillingAddress = ToOrderModel(payment.BillingAddress);
            }
            if (payment.DynamicProperties != null)
            {
                retVal.DynamicProperties = payment.DynamicProperties.Select(ToOrderModel).ToList();
            }
            retVal.TaxDetails = payment.TaxDetails;
            return retVal;
        }

        protected virtual Address ToOrderModel(CoreModule.Core.Common.Address address)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            var retVal = AbstractTypeFactory<Address>.TryCreateInstance();
            retVal.Name = address.Name;
            retVal.Key = null;
            retVal.City = address.City;
            retVal.CountryCode = address.CountryCode;
            retVal.CountryName = address.CountryName;
            retVal.Phone = address.Phone;
            retVal.PostalCode = address.PostalCode;
            retVal.RegionId = address.RegionId;
            retVal.RegionName = address.RegionName;
            retVal.City = address.City;
            retVal.Email = address.Email;
            retVal.FirstName = address.FirstName;
            retVal.LastName = address.LastName;
            retVal.Line1 = address.Line1;
            retVal.Line2 = address.Line2;
            retVal.AddressType = address.AddressType;
            retVal.Organization = address.Organization;
            retVal.OuterId = address.OuterId;

            return retVal;
        }

        protected virtual DynamicObjectProperty ToOrderModel(DynamicObjectProperty item)
        {
            return new DynamicObjectProperty
            {
                Name = item.Name,
                IsDictionary = item.IsDictionary,
                ValueType = item.ValueType,
                Values = item.Values
            };
        }
    }
}
