using System;
using System.Collections.Generic;
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

namespace VirtoCommerce.OrdersModule.Data.Services;

public class CustomerOrderPaymentService(
    IStoreService storeService,
    ICustomerOrderService customerOrderService,
    ICustomerOrderSearchService customerOrderSearchService,
    IValidator<CustomerOrder> customerOrderValidator,
    ISettingsManager settingsManager)
    : ICustomerOrderPaymentService
{
    public virtual async Task<PostProcessPaymentRequestResult> PostProcessPaymentAsync(PaymentParameters paymentParameters)
    {
        ArgumentNullException.ThrowIfNull(paymentParameters);

        var customerOrder = await GetCustomerOrder(paymentParameters);
        if (customerOrder == null)
        {
            throw new InvalidOperationException($"Cannot find order with ID {paymentParameters.OrderId}");
        }

        var store = await storeService.GetByIdAsync(customerOrder.StoreId, nameof(StoreResponseGroup.StoreInfo));
        if (store == null)
        {
            throw new InvalidOperationException($"Cannot find store with ID {customerOrder.StoreId}");
        }

        var inPayments = GetInPayments(customerOrder, paymentParameters);

        foreach (var inPayment in inPayments)
        {
            // Each payment method must check that these parameters are addressed to it
            var paymentMethodValidationResult = inPayment.PaymentMethod.ValidatePostProcessRequest(paymentParameters.Parameters);
            if (!paymentMethodValidationResult.IsSuccess)
            {
                continue;
            }

            var paymentMethodRequest = new PostProcessPaymentRequest
            {
                OrderId = customerOrder.Id,
                Order = customerOrder,
                PaymentId = inPayment.Id,
                Payment = inPayment,
                StoreId = customerOrder.StoreId,
                Store = store,
                OuterId = paymentMethodValidationResult.OuterId,
                Parameters = paymentParameters.Parameters,
            };

            var paymentMethodPostProcessResult = inPayment.PaymentMethod.PostProcessPayment(paymentMethodRequest);
            if (paymentMethodPostProcessResult == null)
            {
                continue;
            }

            var customerOrderValidationResult = await ValidateAsync(customerOrder);
            if (!customerOrderValidationResult.IsValid)
            {
                return new PostProcessPaymentRequestNotValidResult
                {
                    Errors = customerOrderValidationResult.Errors,
                    ErrorMessage = string.Join(" ", customerOrderValidationResult.Errors.Select(x => x.ErrorMessage)),
                };
            }

            await customerOrderService.SaveChangesAsync([customerOrder]);

            // Order number is required
            paymentMethodPostProcessResult.OrderId = customerOrder.Number;

            return paymentMethodPostProcessResult;
        }

        return new PostProcessPaymentRequestResult { ErrorMessage = "Payment method not found" };
    }

    protected virtual async Task<CustomerOrder> GetCustomerOrder(PaymentParameters paymentParameters)
    {
        if (string.IsNullOrEmpty(paymentParameters.OrderId))
        {
            throw new InvalidOperationException("The 'orderid' parameter must be passed");
        }

        // Some payment method require customer number to be passed and returned. First search customer order by number
        var searchCriteria = AbstractTypeFactory<CustomerOrderSearchCriteria>.TryCreateInstance();
        searchCriteria.Number = paymentParameters.OrderId;
        searchCriteria.ResponseGroup = nameof(CustomerOrderResponseGroup.Full);

        // If order is not found by order number, search by order id
        var orders = await customerOrderSearchService.SearchAsync(searchCriteria);

        return orders.Results.FirstOrDefault() ?? await customerOrderService.GetByIdAsync(paymentParameters.OrderId, nameof(CustomerOrderResponseGroup.Full));
    }

    protected virtual IList<PaymentIn> GetInPayments(CustomerOrder customerOrder, PaymentParameters paymentParameters)
    {
        // Need to use concrete  payment method if its code has been passed, otherwise use all order payment methods
        return customerOrder.InPayments
            .Where(x => x.PaymentMethod != null && (string.IsNullOrEmpty(paymentParameters.PaymentMethodCode) || x.GatewayCode.EqualsIgnoreCase(paymentParameters.PaymentMethodCode)))
            .ToList();
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
