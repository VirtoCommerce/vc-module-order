using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.OrdersModule.Data.Services
{
    public class CustomerOrderPaymentService(
        IStoreService storeService,
        ICustomerOrderService customerOrderService,
        ICustomerOrderSearchService customerOrderSearchService,
        IValidator<CustomerOrder> customerOrderValidator,
        ISettingsManager settingsManager)
        : ICustomerOrderPaymentService
    {
        public virtual async Task<PostProcessPaymentRequestResult> PostProcessPaymentAsync(NameValueCollection paymentParameters)
        {
            var orderId = GetOrderId(paymentParameters);
            var paymentMethodCode = GetPaymentMethodCode(paymentParameters);

            if (string.IsNullOrEmpty(orderId))
            {
                throw new InvalidOperationException("The 'orderid' parameter must be passed");
            }

            //some payment method require customer number to be passed and returned. First search customer order by number
            var searchCriteria = AbstractTypeFactory<CustomerOrderSearchCriteria>.TryCreateInstance();
            searchCriteria.Number = orderId;
            searchCriteria.ResponseGroup = CustomerOrderResponseGroup.Full.ToString();
            //if order not found by order number search by order id
            var orders = await customerOrderSearchService.SearchAsync(searchCriteria);
            var customerOrder = orders.Results.FirstOrDefault() ?? await customerOrderService.GetByIdAsync(orderId, CustomerOrderResponseGroup.Full.ToString());

            if (customerOrder == null)
            {
                throw new InvalidOperationException($"Cannot find order with ID {orderId}");
            }

            var store = await storeService.GetByIdAsync(customerOrder.StoreId, StoreResponseGroup.StoreInfo.ToString());
            if (store == null)
            {
                throw new InvalidOperationException($"Cannot find store with ID {customerOrder.StoreId}");
            }

            //Need to use concrete  payment method if it code passed otherwise use all order payment methods
            foreach (var inPayment in customerOrder.InPayments.Where(x => x.PaymentMethod != null && (string.IsNullOrEmpty(paymentMethodCode) || x.GatewayCode.EqualsIgnoreCase(paymentMethodCode))))
            {
                //Each payment method must check that these parameters are addressed to it
                var result = inPayment.PaymentMethod.ValidatePostProcessRequest(paymentParameters);
                if (result.IsSuccess)
                {

                    var request = new PostProcessPaymentRequest
                    {
                        OrderId = customerOrder.Id,
                        Order = customerOrder,
                        PaymentId = inPayment.Id,
                        Payment = inPayment,
                        StoreId = customerOrder.StoreId,
                        Store = store,
                        OuterId = result.OuterId,
                        Parameters = paymentParameters
                    };
                    var retVal = inPayment.PaymentMethod.PostProcessPayment(request);
                    if (retVal != null)
                    {
                        var validationResult = await ValidateAsync(customerOrder);
                        if (!validationResult.IsValid)
                        {
                            return new PostProcessPaymentRequestNotValidResult()
                            {
                                Errors = validationResult.Errors,
                                ErrorMessage = string.Join(" ", validationResult.Errors.Select(x => x.ErrorMessage))
                            };
                        }
                        await customerOrderService.SaveChangesAsync(new[] { customerOrder });

                        // order Number is required
                        retVal.OrderId = customerOrder.Number;
                    }
                    return retVal;
                }
            }
            return new PostProcessPaymentRequestResult { ErrorMessage = "Payment method not found" };
        }

        protected virtual string GetOrderId(NameValueCollection paymentParameters)
        {
            return paymentParameters.Get("orderid");
        }

        protected virtual string GetPaymentMethodCode(NameValueCollection paymentParameters)
        {
            return paymentParameters.Get("code");
        }

        protected virtual async Task<ValidationResult> ValidateAsync(CustomerOrder customerOrder)
        {
            if (await settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.CustomerOrderValidation))
            {
                return await customerOrderValidator.ValidateAsync(customerOrder);
            }

            return new ValidationResult();
        }
    }
}
