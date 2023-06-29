using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Validators;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Data.Services
{
    public class PaymentFlowService : IPaymentFlowService
    {
        private readonly ICrudService<CustomerOrder> _customerOrderService;
        private readonly ICrudService<PaymentIn> _paymentService;
        private readonly ICrudService<Store> _storeService;
        private readonly IValidator<OrderPaymentInfo> _validator;
        private readonly IUniqueNumberGenerator _uniqueNumberGenerator;

        protected virtual string[] CaptureRuleSets => new[] { PaymentRequestValidator.DefaultRuleSet, PaymentRequestValidator.CaptureRuleSet };
        protected virtual string[] RefundRuleSets => new[] { PaymentRequestValidator.DefaultRuleSet, PaymentRequestValidator.RefundRuleSet };

        public PaymentFlowService(
            ICrudService<CustomerOrder> customerOrderService,
            ICrudService<PaymentIn> paymentService,
            ICrudService<Store> storeService,
            IValidator<OrderPaymentInfo> validator,
            IUniqueNumberGenerator uniqueNumberGenerator)
        {
            _customerOrderService = customerOrderService;
            _paymentService = paymentService;
            _storeService = storeService;
            _validator = validator;
            _uniqueNumberGenerator = uniqueNumberGenerator;
        }

        public virtual async Task<RefundOrderPaymentResult> RefundPaymentAsync(RefundOrderPaymentRequest request)
        {
            var result = AbstractTypeFactory<RefundOrderPaymentResult>.TryCreateInstance();

            var paymentInfo = await GetPaymentInfoAsync(request, PaymentStatus.Paid, PaymentStatus.PartiallyRefunded);

            // validate payment
            var validationResult = _validator.Validate(paymentInfo, options => options.IncludeRuleSets(RefundRuleSets));
            if (!validationResult.IsValid)
            {
                var error = validationResult.Errors.FirstOrDefault();
                result.ErrorMessage = error?.ErrorMessage;
                result.ErrorCode = error?.ErrorCode;

                return result;
            }

            Refund refund = null;

            await HandleConcurrentUpdate(
                () =>
                {
                    // create and save refund
                    refund = CreateRefund(paymentInfo.Payment, paymentInfo.Store, request);

                    if (paymentInfo.Payment.Refunds == null)
                    {
                        paymentInfo.Payment.Refunds = new List<Refund>();
                    }
                    paymentInfo.Payment.Refunds.Add(refund);

                    return Task.FromResult(refund);
                },
                async () =>
                {
                    await _customerOrderService.SaveChangesAsync(new[] { paymentInfo.CustomerOrder });
                },
                async () =>
                {
                    GenericCachingRegion<CustomerOrder>.ExpireTokenForKey(paymentInfo.CustomerOrder.Id);

                    paymentInfo = await GetPaymentInfoAsync(request, PaymentStatus.Paid, PaymentStatus.PartiallyRefunded);
                }
            );

            // call payment method refund
            var refundRequest = GetRefundPaymentRequest(paymentInfo, request);
            var refundResult = default(RefundPaymentRequestResult);

            try
            {
                refundResult = paymentInfo.Payment.PaymentMethod.RefundProcessPayment(refundRequest);
            }
            catch (Exception ex)
            {
                result.ErrorCode = PaymentFlowErrorCodes.PaymentFailed;
                result.ErrorMessage = ex.Message;

                return result;
            }


            await HandleConcurrentUpdate(
               () =>
               {
                   if (refundResult.IsSuccess)
                   {
                       refund.Status = refundResult.NewRefundStatus.ToString();
                       result.RefundStatus = refund.Status;
                       result.Succeeded = true;
                   }
                   else
                   {
                       refund.Status = RefundStatus.Rejected.ToString();
                       result.ErrorCode = PaymentFlowErrorCodes.PaymentFailed;
                       result.ErrorMessage = refundResult.ErrorMessage;
                   }

                   return Task.FromResult(refund);
               },
               async () =>
               {
                   await _customerOrderService.SaveChangesAsync(new[] { paymentInfo.CustomerOrder });
               },
               async () =>
               {
                   GenericCachingRegion<CustomerOrder>.ExpireTokenForKey(paymentInfo.CustomerOrder.Id);

                   paymentInfo = await GetPaymentInfoAsync(request, PaymentStatus.Paid, PaymentStatus.PartiallyRefunded);
                   refund = paymentInfo.Payment.Refunds.First(c => c.TransactionId == request.TransactionId);
               }
           );


            return result;
        }


        public virtual async Task<CaptureOrderPaymentResult> CapturePaymentAsync(CaptureOrderPaymentRequest request)
        {
            // Step 1. Validat, Add a new capture request and save to DB
            var result = AbstractTypeFactory<CaptureOrderPaymentResult>.TryCreateInstance();

            var paymentInfo = await GetPaymentInfoAsync(request, PaymentStatus.Authorized);

            // validate payment
            var validationResult = _validator.Validate(paymentInfo, options => options.IncludeRuleSets(CaptureRuleSets));
            if (!validationResult.IsValid)
            {
                var error = validationResult.Errors.FirstOrDefault();
                result.ErrorMessage = error?.ErrorMessage;
                result.ErrorCode = error?.ErrorCode;

                return result;
            }

            Capture capture = null;

            await HandleConcurrentUpdate(
                () =>
                {
                    capture = CreateCapture(paymentInfo.Payment, paymentInfo.Store, request);

                    if (paymentInfo.Payment.Captures == null)
                    {
                        paymentInfo.Payment.Captures = new List<Capture>();
                    }
                    paymentInfo.Payment.Captures.Add(capture);

                    return Task.FromResult(capture);
                },
                async () =>
                {
                    await _customerOrderService.SaveChangesAsync(new[] { paymentInfo.CustomerOrder });
                },
                async () =>
                {
                    GenericCachingRegion<CustomerOrder>.ExpireTokenForKey(paymentInfo.CustomerOrder.Id);
                    paymentInfo = await GetPaymentInfoAsync(request, PaymentStatus.Authorized);
                }
            );

            var captureRequest = GetCapturePaymentRequest(paymentInfo, request);
            var captureResult = default(CapturePaymentRequestResult);

            try
            {
                captureResult = paymentInfo.Payment.PaymentMethod.CaptureProcessPayment(captureRequest);
            }
            catch (Exception ex)
            {
                result.ErrorCode = PaymentFlowErrorCodes.PaymentFailed;
                result.ErrorMessage = ex.Message;

                return result;
            }

            await HandleConcurrentUpdate(
                () =>
                {
                    if (captureResult.IsSuccess)
                    {
                        capture.Status = "Processed";
                        paymentInfo.Payment.Status = captureResult.NewPaymentStatus.ToString();
                        result.PaymentStatus = paymentInfo.Payment.Status;
                        result.Succeeded = true;
                    }
                    else
                    {
                        capture.Status = "Rejected";
                        result.ErrorCode = PaymentFlowErrorCodes.PaymentFailed;
                        result.ErrorMessage = captureResult.ErrorMessage;
                    }

                    return Task.FromResult(capture);
                },
                async () =>
                {
                    await _customerOrderService.SaveChangesAsync(new[] { paymentInfo.CustomerOrder });
                },
                async () =>
                {
                    GenericCachingRegion<CustomerOrder>.ExpireTokenForKey(paymentInfo.CustomerOrder.Id);

                    paymentInfo = await GetPaymentInfoAsync(request, PaymentStatus.Authorized);
                    capture = paymentInfo.Payment.Captures.First(c => c.TransactionId == request.TransactionId);
                }
            );


            return result;
        }

        protected virtual async Task<OrderPaymentInfo> GetPaymentInfoAsync(OrderPaymentRequest request, params PaymentStatus[] allowedStatuses)
        {
            var result = AbstractTypeFactory<OrderPaymentInfo>.TryCreateInstance();

            if (!string.IsNullOrEmpty(request.OrderId))
            {
                result.CustomerOrder = await _customerOrderService.GetByIdAsync(request.OrderId, CustomerOrderResponseGroup.Full.ToString());

                result.Payment = string.IsNullOrEmpty(request.PaymentId)
                    ? result.CustomerOrder?.InPayments?.FirstOrDefault(x => allowedStatuses.Contains(x.PaymentStatus))
                    : result.CustomerOrder?.InPayments?.FirstOrDefault(x => x.Id == request.PaymentId && allowedStatuses.Contains(x.PaymentStatus));
            }
            else if (!string.IsNullOrEmpty(request.PaymentId))
            {
                result.Payment = await _paymentService.GetByIdAsync(request.PaymentId);
                result.CustomerOrder = await _customerOrderService.GetByIdAsync(result.Payment?.OrderId, CustomerOrderResponseGroup.Full.ToString());

                // take payment from order since Order payment contains instanced PaymentMethod (payment taken from service doesn't)
                result.Payment = result.CustomerOrder?.InPayments?.FirstOrDefault(x => x.Id == request.PaymentId && allowedStatuses.Contains(x.PaymentStatus));
            }

            result.Store = await _storeService.GetByIdAsync(result.CustomerOrder?.StoreId, StoreResponseGroup.StoreInfo.ToString());

            return result;
        }

        protected virtual CapturePaymentRequest GetCapturePaymentRequest(OrderPaymentInfo paymentInfo, CaptureOrderPaymentRequest request)
        {
            var result = AbstractTypeFactory<CapturePaymentRequest>.TryCreateInstance();

            FillPaymentRequestBase(paymentInfo, result);

            result.CaptureAmount = request.Amount ?? paymentInfo.Payment.Sum;
            result.OuterId = request.OuterId;

            if (!string.IsNullOrEmpty(request.CaptureDetails))
            {
                result.Parameters = new NameValueCollection
                {
                    { nameof(request.CaptureDetails), request.CaptureDetails }
                };
            }

            return result;
        }

        protected virtual RefundPaymentRequest GetRefundPaymentRequest(OrderPaymentInfo paymentInfo, RefundOrderPaymentRequest request)
        {
            var result = AbstractTypeFactory<RefundPaymentRequest>.TryCreateInstance();

            FillPaymentRequestBase(paymentInfo, result);

            result.AmountToRefund = request.Amount ?? paymentInfo.Payment.Sum;
            result.Reason = request.ReasonCode;
            result.Notes = request.ReasonMessage;
            result.OuterId = request.OuterId;

            return result;
        }

        protected virtual Refund CreateRefund(PaymentIn payment, Store store, RefundOrderPaymentRequest request)
        {
            var refund = AbstractTypeFactory<Refund>.TryCreateInstance();

            var numberTemplate = store.Settings.GetSettingValue(
                Core.ModuleConstants.Settings.General.RefundNewNumberTemplate.Name,
                Core.ModuleConstants.Settings.General.RefundNewNumberTemplate.DefaultValue);
            refund.Number = _uniqueNumberGenerator.GenerateNumber(numberTemplate.ToString());

            refund.Amount = request.Amount ?? payment.Sum;
            refund.ReasonCode = EnumUtility.SafeParse(request.ReasonCode, RefundReasonCode.Other);
            refund.ReasonMessage = request.ReasonMessage;
            refund.Comment = request.ReasonMessage;
            refund.OuterId = request.OuterId;
            refund.TransactionId = request.TransactionId;

            refund.Status = RefundStatus.Pending.ToString();
            refund.Currency = payment.Currency;
            refund.CustomerOrderId = payment.OrderId;
            refund.VendorId = payment.VendorId;

            return refund;
        }

        protected virtual Capture CreateCapture(PaymentIn payment, Store store, CaptureOrderPaymentRequest request)
        {
            var capture = AbstractTypeFactory<Capture>.TryCreateInstance();

            var numberTemplate = store.Settings.GetSettingValue(
                Core.ModuleConstants.Settings.General.CaptureNewNumberTemplate.Name,
                Core.ModuleConstants.Settings.General.CaptureNewNumberTemplate.DefaultValue);
            capture.Number = _uniqueNumberGenerator.GenerateNumber(numberTemplate.ToString());

            capture.Amount = request.Amount ?? payment.Sum;
            capture.Comment = request.CaptureDetails;
            capture.OuterId = request.OuterId;
            capture.TransactionId = request.TransactionId;

            capture.Status = "Pending";
            capture.Currency = payment.Currency;
            capture.CustomerOrderId = payment.OrderId;
            capture.VendorId = payment.VendorId;

            return capture;
        }

        protected static async Task HandleConcurrentUpdate(Func<Task> initialAction, Func<Task> saveToDbAction, Func<Task> callbackAction, int maxAttempts = 3)
        {
            var attemptCount = 0;
            var isSaveSuccessful = false;

            while (!isSaveSuccessful && attemptCount < maxAttempts)
            {
                try
                {
                    await initialAction(); // Execute the initial business logic action
                    await saveToDbAction(); // Execute the save to DB action

                    isSaveSuccessful = true; // Mark the save operation as successful
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Execute the callback action after the concurrency exception occurs
                    await callbackAction();

                    attemptCount++; // Increment the attempt count

                    if (attemptCount == maxAttempts)
                    {
                        throw; // Throw the original exception if all retries are unsuccessful
                    }
                }
            }
        }

        private static void FillPaymentRequestBase(OrderPaymentInfo paymentInfo, PaymentRequestBase result)
        {
            result.OrderId = paymentInfo.CustomerOrder.Id;
            result.Order = paymentInfo.CustomerOrder;
            result.PaymentId = paymentInfo.Payment.Id;
            result.Payment = paymentInfo.Payment;
            result.StoreId = paymentInfo.Store.Id;
            result.Store = paymentInfo.Store;
        }
    }
}
