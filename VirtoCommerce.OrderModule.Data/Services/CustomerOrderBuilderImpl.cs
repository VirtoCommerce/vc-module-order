using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Order.Services;
using VirtoCommerce.Domain.Store.Services;
using Omu.ValueInjecter;
using cartModel = VirtoCommerce.Domain.Cart.Model;
using orderModel = VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Domain.Payment.Model;

namespace VirtoCommerce.OrderModule.Data.Services
{
    public class CustomerOrderBuilderImpl : ICustomerOrderBuilder
    {
        private ICustomerOrderService _customerOrderService;
        private IStoreService _storeService;
        public CustomerOrderBuilderImpl(ICustomerOrderService customerOrderService, IStoreService storeService)
        {
            _storeService = storeService;
            _customerOrderService = customerOrderService;
        }

        #region ICustomerOrderConverter Members

        public virtual orderModel.CustomerOrder PlaceCustomerOrderFromCart(cartModel.ShoppingCart cart)
        {
            var customerOrder = ConvertCartToOrder(cart);
            _customerOrderService.SaveChanges(new[] { customerOrder });

            customerOrder = _customerOrderService.GetByIds(new[] { customerOrder.Id }).FirstOrDefault();

            return customerOrder;
        }

        #endregion
        protected virtual orderModel.CustomerOrder ConvertCartToOrder(cartModel.ShoppingCart cart)
        {
            var retVal = AbstractTypeFactory<orderModel.CustomerOrder>.TryCreateInstance();

            retVal.Comment = cart.Comment;
            retVal.Currency = cart.Currency;
            retVal.ChannelId = cart.ChannelId;
            retVal.CustomerId = cart.CustomerId;
            retVal.CustomerName = cart.CustomerName;
            retVal.DiscountAmount = cart.DiscountAmount;
            retVal.OrganizationId = cart.OrganizationId;
            retVal.StoreId = cart.StoreId;
            retVal.TaxPercentRate = cart.TaxPercentRate;
            retVal.TaxType = cart.TaxType;
            retVal.LanguageCode = cart.LanguageCode;
            
            retVal.Status = "New";

            if (cart.Items != null)
            {
                retVal.Items = cart.Items.Select(x => ToOrderModel(x)).ToList();
            }
            if (cart.Discounts != null)
            {
                retVal.Discounts = cart.Discounts.Select(x => ToOrderModel(x)).ToList();
            }

            if (cart.Addresses != null)
            {
                retVal.Addresses = cart.Addresses.ToList();
            }

            if (cart.Shipments != null)
            {
                retVal.Shipments = cart.Shipments.Select(x => ToOrderModel(x)).ToList();
                //Add shipping address to order
                retVal.Addresses.AddRange(retVal.Shipments.Where(x => x.DeliveryAddress != null).Select(x => x.DeliveryAddress));
                //Redistribute order line items to shipment if cart shipment items empty 
                //var shipment = retVal.Shipments.FirstOrDefault();
                //if (shipment != null && shipment.Items.IsNullOrEmpty())
                //{
                //    shipment.Items = retVal.Items.Select(x => new Domain.Order.Model.ShipmentItem { LineItem = x, Quantity = x.Quantity }).ToList();
                //}
            }
            if (cart.Payments != null)
            {
                retVal.InPayments = new List<orderModel.PaymentIn>();
                foreach (var payment in cart.Payments)
                {
                    var paymentIn = ToOrderModel(payment);
                    paymentIn.CustomerId = cart.CustomerId;
                    retVal.InPayments.Add(paymentIn);
                    if(payment.BillingAddress != null)
                    {
                        retVal.Addresses.Add(payment.BillingAddress);
                    }
                }
            }

            //Save only disctinct addresses for order
            retVal.Addresses = retVal.Addresses.Distinct().ToList();
            retVal.TaxDetails = cart.TaxDetails;
            return retVal;
        }

        protected virtual orderModel.LineItem ToOrderModel(cartModel.LineItem lineItem)
        {
            if (lineItem == null)
                throw new ArgumentNullException("lineItem");

            var retVal = AbstractTypeFactory<orderModel.LineItem>.TryCreateInstance();

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

            retVal.DiscountAmount = lineItem.DiscountAmount;
            retVal.Price = lineItem.ListPrice;
          
            retVal.FulfillmentLocationCode = lineItem.FulfillmentLocationCode;
            retVal.DynamicProperties = null; //to prevent copy dynamic properties from ShoppingCart LineItem to Order LineItem
            if (lineItem.Discounts != null)
            {
                retVal.Discounts = lineItem.Discounts.Select(x => ToOrderModel(x)).ToList();
            }
            retVal.TaxDetails = lineItem.TaxDetails;
            return retVal;
        }

        protected virtual orderModel.Discount ToOrderModel(cartModel.Discount discount)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            var retVal = AbstractTypeFactory<Domain.Order.Model.Discount>.TryCreateInstance();
            if(!string.IsNullOrEmpty(discount.Coupon))
            {
                retVal.Coupon = new orderModel.Coupon
                {
                    Code = discount.Coupon,
                    IsValid = true
                };
            }
            retVal.Currency = discount.Currency;
            retVal.Description = discount.Description;
            retVal.DiscountAmount = discount.DiscountAmount;
            retVal.DiscountAmountWithTax = discount.DiscountAmountWithTax;
            retVal.PromotionId = discount.PromotionId;

            return retVal;
        }

        protected virtual orderModel.Shipment ToOrderModel(cartModel.Shipment shipment)
        {
            var retVal = AbstractTypeFactory<orderModel.Shipment>.TryCreateInstance();
            retVal.Currency = shipment.Currency;
            retVal.DiscountAmount = shipment.DiscountAmount;
            retVal.Height = shipment.Height;
            retVal.Length = shipment.Length;
            retVal.MeasureUnit = shipment.MeasureUnit;
            retVal.ShipmentMethodCode = shipment.ShipmentMethodCode;
            retVal.ShipmentMethodOption = shipment.ShipmentMethodOption;
            retVal.Sum = shipment.Total;
            retVal.Weight = shipment.Weight;
            retVal.WeightUnit = shipment.WeightUnit;
            retVal.Width = shipment.Width;            
            retVal.TaxPercentRate = shipment.TaxPercentRate;
            retVal.DiscountAmount = shipment.DiscountAmount;
            retVal.Price = shipment.Price;
            retVal.Status = "New";
            if (shipment.DeliveryAddress != null)
            {
                retVal.DeliveryAddress = shipment.DeliveryAddress;
            }
            //if (shipment.Items != null)
            //{
            //    retVal.Items = shipment.Items.Select(x => ToOrderModel(x)).ToList();
            //}
            if (shipment.Discounts != null)
            {
                retVal.Discounts = shipment.Discounts.Select(x => ToOrderModel(x)).ToList();
            }
            retVal.TaxDetails = shipment.TaxDetails;
            return retVal;
        }

        protected virtual orderModel.ShipmentItem ToOrderModel(cartModel.ShipmentItem shipmentItem)
        {
            if (shipmentItem == null)
                throw new ArgumentNullException("shipmentItem");

            var retVal = AbstractTypeFactory<orderModel.ShipmentItem>.TryCreateInstance();
            retVal.InjectFrom(shipmentItem);
            return retVal;
        }

        protected virtual orderModel.PaymentIn ToOrderModel(cartModel.Payment payment)
        {
            if (payment == null)
                throw new ArgumentNullException("payment");

            var retVal = AbstractTypeFactory<orderModel.PaymentIn>.TryCreateInstance();
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
                retVal.BillingAddress = payment.BillingAddress;
            }
            retVal.TaxDetails = payment.TaxDetails;
            return retVal;
        }

    }
}
