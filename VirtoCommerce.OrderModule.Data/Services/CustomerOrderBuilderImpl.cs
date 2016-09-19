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
            retVal.InjectFrom(cart);
            retVal.Status = "New";

            if (cart.Items != null)
            {
                retVal.Items = cart.Items.Select(x => ToOrderModel(x)).ToList();
            }
            if (cart.Discounts != null)
            {
                retVal.Discount = cart.Discounts.Select(x => ToOrderModel(x)).FirstOrDefault();
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

            var retVal = new Domain.Order.Model.LineItem();
            retVal.InjectFrom(lineItem);
            retVal.Id = null;

            retVal.Price = lineItem.ListPrice;
          
            retVal.FulfillmentLocationCode = lineItem.FulfillmentLocationCode;
            retVal.DynamicProperties = null; //to prevent copy dynamic properties from ShoppingCart LineItem to Order LineItem
            if (lineItem.Discounts != null)
            {
                retVal.Discount = lineItem.Discounts.Select(x => ToOrderModel(x)).FirstOrDefault();
            }
            retVal.TaxDetails = lineItem.TaxDetails;
            return retVal;
        }

        protected virtual orderModel.Discount ToOrderModel(cartModel.Discount discount)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            var retVal = new Domain.Order.Model.Discount();
            retVal.InjectFrom(discount);
            return retVal;
        }

        protected virtual orderModel.Shipment ToOrderModel(cartModel.Shipment shipment)
        {
            var retVal = AbstractTypeFactory<orderModel.Shipment>.TryCreateInstance();
            retVal.InjectFrom(shipment);
            retVal.Id = null;

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
                retVal.Discount = shipment.Discounts.Select(x => ToOrderModel(x)).FirstOrDefault();
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
            retVal.InjectFrom(payment);
            retVal.Id = null;

            retVal.GatewayCode = payment.PaymentGatewayCode;
            retVal.Sum = payment.Amount;
            retVal.PaymentStatus = PaymentStatus.New;
            if (payment.BillingAddress != null)
            {
                retVal.BillingAddress = payment.BillingAddress;
            }
            return retVal;
        }

    }
}
