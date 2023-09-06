using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Polly;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Validators;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.OrdersModule.Data.Services
{
    public class PaymentFlowService : IPaymentFlowService
    {
        private readonly ICustomerOrderService _customerOrderService;
        private readonly IPaymentService _paymentService;
        private readonly IStoreService _storeService;
        private readonly IValidator<OrderPaymentInfo> _validator;
        private readonly IUniqueNumberGenerator _uniqueNumberGenerator;

        protected virtual string[] CaptureRuleSets => new[] { PaymentRequestValidator.DefaultRuleSet, PaymentRequestValidator.CaptureRuleSet };
        protected virtual string[] RefundRuleSets => new[] { PaymentRequestValidator.DefaultRuleSet, PaymentRequestValidator.RefundRuleSet };

        protected virtual PaymentStatus[] CaptureAllowedPaymentStatuses => new[] { PaymentStatus.Authorized, PaymentStatus.Paid };
        protected virtual PaymentStatus[] RefundAllowedPaymentStatuses => new[] { PaymentStatus.Paid, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded };

        public PaymentFlowService(
            ICustomerOrderService customerOrderService,
            IPaymentService paymentService,
            IStoreService storeService,
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
            var dbConcurrencyRetryPolicy = Policy.Handle<DbUpdateConcurrencyException>().WaitAndRetryAsync(retryCount: 5, _ => TimeSpan.FromMilliseconds(500));
            var result = await dbConcurrencyRetryPolicy.ExecuteAsync(async () => await CreateRefundDocument(request));

            if (!result.Succeeded)
            {
                return result;
            }

            RefundPaymentRequestResult refundResult;
            try
            {
                refundResult = await ProcessRefund(request);
            }
            catch (Exception ex)
            {
                result.Succeeded = false;
                result.ErrorCode = PaymentFlowErrorCodes.PaymentFailed;
                result.ErrorMessage = ex.Message;

                return result;
            }

            result = await dbConcurrencyRetryPolicy.ExecuteAsync(async () => await SaveResultToRefundDocument(request, refundResult));

            return result;
        }

        protected virtual async Task<RefundOrderPaymentResult> CreateRefundDocument(RefundOrderPaymentRequest request)
        {
            var result = AbstractTypeFactory<RefundOrderPaymentResult>.TryCreateInstance();

            var paymentInfo = await GetPaymentInfoAsync(request, RefundAllowedPaymentStatuses);

            // validate payment
            var validationResult = await _validator.ValidateAsync(paymentInfo, options => options.IncludeRuleSets(RefundRuleSets));
            if (!validationResult.IsValid)
            {
                var error = validationResult.Errors.FirstOrDefault();
                result.ErrorMessage = error?.ErrorMessage;
                result.ErrorCode = error?.ErrorCode;

                return result;
            }

            var refund = CreateRefund(paymentInfo.Payment, paymentInfo.Store, request);

            paymentInfo.Payment.Refunds ??= new List<Refund>();
            paymentInfo.Payment.Refunds.Add(refund);
            await _customerOrderService.SaveChangesAsync(new[] { paymentInfo.CustomerOrder });

            result.Succeeded = true;

            return result;
        }

        protected virtual async Task<RefundPaymentRequestResult> ProcessRefund(RefundOrderPaymentRequest request)
        {
            var paymentInfo = await GetPaymentInfoAsync(request, RefundAllowedPaymentStatuses);
            var refundRequest = GetRefundPaymentRequest(paymentInfo, request);

            return paymentInfo.Payment.PaymentMethod.RefundProcessPayment(refundRequest);
        }

        protected virtual async Task<RefundOrderPaymentResult> SaveResultToRefundDocument(RefundOrderPaymentRequest request, RefundPaymentRequestResult refundResult)
        {
            var result = AbstractTypeFactory<RefundOrderPaymentResult>.TryCreateInstance();

            var paymentInfo = await GetPaymentInfoAsync(request, RefundAllowedPaymentStatuses);
            var refund = paymentInfo.Payment.Refunds.First(c => c.TransactionId == request.TransactionId);

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

            await _customerOrderService.SaveChangesAsync(new[] { paymentInfo.CustomerOrder });

            return result;
        }


        // Validate, add new capture request, save to DB
        public virtual async Task<CaptureOrderPaymentResult> CreateCaptureDocument(CaptureOrderPaymentRequest request)
        {
            var result = AbstractTypeFactory<CaptureOrderPaymentResult>.TryCreateInstance();

            // Paid status is also available for capture operation, since in case of multiple captures per single payment we don't know when it will be the last capture
            var paymentInfo = await GetPaymentInfoAsync(request, CaptureAllowedPaymentStatuses);

            // validate payment
            var validationResult = await _validator.ValidateAsync(paymentInfo, options => options.IncludeRuleSets(CaptureRuleSets));
            if (!validationResult.IsValid)
            {
                var error = validationResult.Errors.FirstOrDefault();
                result.ErrorMessage = error?.ErrorMessage;
                result.ErrorCode = error?.ErrorCode;

                return result;
            }

            var capture = CreateCapture(paymentInfo.Payment, paymentInfo.Store, request);

            paymentInfo.Payment.Captures ??= new List<Capture>();
            paymentInfo.Payment.Captures.Add(capture);
            await _customerOrderService.SaveChangesAsync(new[] { paymentInfo.CustomerOrder });

            result.Succeeded = true;

            return result;
        }

        public virtual async Task<CapturePaymentRequestResult> ProcessCapture(CaptureOrderPaymentRequest request)
        {
            // Paid status is also available for capture operation, since in case of multiple captures per single payment we don't know when it will be the last capture
            var paymentInfo = await GetPaymentInfoAsync(request, CaptureAllowedPaymentStatuses);
            var captureRequest = GetCapturePaymentRequest(paymentInfo, request);

            return paymentInfo.Payment.PaymentMethod.CaptureProcessPayment(captureRequest);
        }

        public virtual async Task<CaptureOrderPaymentResult> SaveResultToCaptureDocument(CaptureOrderPaymentRequest request, CapturePaymentRequestResult captureResult)
        {
            var result = AbstractTypeFactory<CaptureOrderPaymentResult>.TryCreateInstance();

            var paymentInfo = await GetPaymentInfoAsync(request, CaptureAllowedPaymentStatuses);
            var capture = paymentInfo.Payment.Captures.First(c => c.TransactionId == request.TransactionId);

            if (captureResult.IsSuccess)
            {
                capture.Status = CaptureStatus.Processed.ToString();
                paymentInfo.Payment.Status = captureResult.NewPaymentStatus.ToString();
                result.PaymentStatus = paymentInfo.Payment.Status;
                result.Succeeded = true;
            }
            else
            {
                capture.Status = CaptureStatus.Rejected.ToString();
                result.ErrorCode = PaymentFlowErrorCodes.PaymentFailed;
                result.ErrorMessage = captureResult.ErrorMessage;
            }

            await _customerOrderService.SaveChangesAsync(new[] { paymentInfo.CustomerOrder });

            return result;
        }

        public virtual async Task<CaptureOrderPaymentResult> CapturePaymentAsync(CaptureOrderPaymentRequest request)
        {
            var dbConcurrencyRetryPolicy = Policy.Handle<DbUpdateConcurrencyException>().WaitAndRetryAsync(retryCount: 5, _ => TimeSpan.FromMilliseconds(500));
            var result = await dbConcurrencyRetryPolicy.ExecuteAsync(async () => await CreateCaptureDocument(request));

            if (!result.Succeeded)
            {
                return result;
            }

            CapturePaymentRequestResult captureResult;
            try
            {
                captureResult = await ProcessCapture(request);
            }
            catch (Exception ex)
            {
                result.Succeeded = false;
                result.ErrorCode = PaymentFlowErrorCodes.PaymentFailed;
                result.ErrorMessage = ex.Message;

                return result;
            }

            result = await dbConcurrencyRetryPolicy.ExecuteAsync(async () => await SaveResultToCaptureDocument(request, captureResult));

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

            var numberTemplate = store.Settings.GetValue<string>(Core.ModuleConstants.Settings.General.RefundNewNumberTemplate);
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

            var numberTemplate = store.Settings.GetValue<string>(Core.ModuleConstants.Settings.General.CaptureNewNumberTemplate);
            capture.Number = _uniqueNumberGenerator.GenerateNumber(numberTemplate.ToString());

            capture.Amount = request.Amount ?? payment.Sum;
            capture.Comment = request.CaptureDetails;
            capture.OuterId = request.OuterId;
            capture.TransactionId = request.TransactionId;

            capture.Status = CaptureStatus.Pending.ToString();
            capture.Currency = payment.Currency;
            capture.CustomerOrderId = payment.OrderId;
            capture.VendorId = payment.VendorId;

            return capture;
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
