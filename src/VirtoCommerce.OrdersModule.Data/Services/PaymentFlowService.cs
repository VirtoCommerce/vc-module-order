using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polly;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Validators;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DistributedLock;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.OrdersModule.Data.Services
{
    public class PaymentFlowService(
        ICustomerOrderService customerOrderService,
        IPaymentService paymentService,
        IStoreService storeService,
        IValidator<OrderPaymentInfo> validator,
        ITenantUniqueNumberGenerator uniqueNumberGenerator,
        IOptions<PaymentDistributedLockOptions> paymentDistributedLockOptions,
        IDistributedLockService distributedLockService) : IPaymentFlowService
    {
        private readonly PaymentDistributedLockOptions _lockOptions = paymentDistributedLockOptions.Value;

        protected virtual string[] CaptureRuleSets => [PaymentRequestValidator.DefaultRuleSet, PaymentRequestValidator.CaptureRuleSet];
        protected virtual string[] RefundRuleSets => [PaymentRequestValidator.DefaultRuleSet, PaymentRequestValidator.RefundRuleSet];

        protected virtual PaymentStatus[] CaptureAllowedPaymentStatuses => [PaymentStatus.Authorized, PaymentStatus.Paid];
        protected virtual PaymentStatus[] RefundAllowedPaymentStatuses => [PaymentStatus.Paid, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded];

        public virtual async Task<string> GetLockResourceKeyAsync(OrderPaymentRequest request)
        {
            string partKey = null;

            if (!string.IsNullOrEmpty(request.OrderId))
            {
                partKey = request.OrderId;
            }
            else if (!string.IsNullOrEmpty(request.PaymentId))
            {
                var payment = await paymentService.GetByIdAsync(request.PaymentId);
                partKey = payment?.OrderId;
            }

            return string.IsNullOrEmpty(partKey)
                ? throw new InvalidOperationException("Unable to determine lock resource key: the parameter was not passed or the object was not found.")
                : $"{nameof(CustomerOrder)}:{partKey}";
        }

        #region Refund

        public virtual async Task<RefundOrderPaymentResult> RefundPaymentAsync(RefundOrderPaymentRequest request)
        {
            try
            {
                var resourceKey = await GetLockResourceKeyAsync(request);

                if (string.IsNullOrEmpty(resourceKey))
                {
                    throw new InvalidOperationException("Lock resource key is null or empty.");
                }

                var result = await distributedLockService.ExecuteAsync(resourceKey,
                    () => RefundPaymentInternalAsync(request),
                    _lockOptions.LockTimeout,
                    _lockOptions.TryLockTimeout,
                    _lockOptions.RetryInterval);

                return result;
            }
            catch (Exception ex)
            {
                return new RefundOrderPaymentResult
                {
                    Succeeded = false,
                    ErrorCode = PaymentFlowErrorCodes.PaymentFailed,
                    ErrorMessage = ex.Message
                };
            }
        }

        protected virtual async Task<RefundOrderPaymentResult> RefundPaymentInternalAsync(RefundOrderPaymentRequest request)
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
            var validationResult = await validator.ValidateAsync(paymentInfo, options => options.IncludeRuleSets(RefundRuleSets));
            if (!validationResult.IsValid)
            {
                var error = validationResult.Errors.FirstOrDefault();
                result.ErrorMessage = error?.ErrorMessage;
                result.ErrorCode = error?.ErrorCode;

                return result;
            }

            paymentInfo.Payment.Refunds ??= new List<Refund>();

            // Allows to Update and Retry Rejected Refund document.
            var refund = paymentInfo.Payment.Refunds.FirstOrDefault(r => r.TransactionId == request.TransactionId
                && r.Status != nameof(RefundStatus.Processed));

            if (refund != null)
            {
                UpdateRefund(refund, paymentInfo.Payment, paymentInfo.Store, request);
            }
            else
            {
                refund = CreateRefund(paymentInfo.Payment, paymentInfo.Store, request);
                paymentInfo.Payment.Refunds.Add(refund);
            }

            await customerOrderService.SaveChangesAsync([paymentInfo.CustomerOrder]);

            result.Succeeded = true;

            return result;
        }

        protected virtual void UpdateRefund(Refund refund, PaymentIn payment, Store store, RefundOrderPaymentRequest request)
        {
            refund.Amount = request.Amount ?? payment.Sum;
            refund.ReasonCode = EnumUtility.SafeParse(request.ReasonCode, RefundReasonCode.Other);
            refund.ReasonMessage = request.ReasonMessage;
            refund.Comment = request.ReasonMessage;
            refund.OuterId = request.OuterId;
            refund.TransactionId = request.TransactionId;

            refund.Status = nameof(RefundStatus.Pending);
            refund.Currency = payment.Currency;
            refund.CustomerOrderId = payment.OrderId;
            refund.VendorId = payment.VendorId;
        }

        protected virtual Refund CreateRefund(PaymentIn payment, Store store, RefundOrderPaymentRequest request)
        {
            var refund = AbstractTypeFactory<Refund>.TryCreateInstance();

            var numberTemplate = store.Settings.GetValue<string>(Core.ModuleConstants.Settings.General.RefundNewNumberTemplate);
            refund.Number = uniqueNumberGenerator.GenerateNumber(store.Id, numberTemplate);

            refund.Amount = request.Amount ?? payment.Sum;
            refund.ReasonCode = EnumUtility.SafeParse(request.ReasonCode, RefundReasonCode.Other);
            refund.ReasonMessage = request.ReasonMessage;
            refund.Comment = request.ReasonMessage;
            refund.OuterId = request.OuterId;
            refund.TransactionId = request.TransactionId;

            refund.Status = nameof(RefundStatus.Pending);
            refund.Currency = payment.Currency;
            refund.CustomerOrderId = payment.OrderId;
            refund.VendorId = payment.VendorId;

            return refund;
        }

        protected virtual async Task<RefundPaymentRequestResult> ProcessRefund(RefundOrderPaymentRequest request)
        {
            var paymentInfo = await GetPaymentInfoAsync(request, RefundAllowedPaymentStatuses);
            var refundRequest = GetRefundPaymentRequest(paymentInfo, request);

            return paymentInfo.Payment.PaymentMethod.RefundProcessPayment(refundRequest);
        }

        protected virtual RefundPaymentRequest GetRefundPaymentRequest(OrderPaymentInfo paymentInfo, RefundOrderPaymentRequest request)
        {
            var result = AbstractTypeFactory<RefundPaymentRequest>.TryCreateInstance();

            FillPaymentRequestBase(paymentInfo, result);

            result.AmountToRefund = request.Amount ?? paymentInfo.Payment.Sum;
            result.Reason = request.ReasonCode;
            result.Notes = request.ReasonMessage;
            result.OuterId = request.OuterId;

            result.Parameters ??= [];

            result.Parameters.Add(nameof(request.TransactionId), request.TransactionId ?? string.Empty);

            return result;
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
                refund.Status = nameof(RefundStatus.Rejected);
                result.ErrorCode = PaymentFlowErrorCodes.PaymentFailed;
                result.ErrorMessage = refundResult.ErrorMessage;
            }

            await customerOrderService.SaveChangesAsync([paymentInfo.CustomerOrder]);

            return result;
        }

        #endregion

        #region Capture

        public virtual async Task<CaptureOrderPaymentResult> CapturePaymentAsync(CaptureOrderPaymentRequest request)
        {
            try
            {
                var resourceKey = await GetLockResourceKeyAsync(request);

                if (string.IsNullOrEmpty(resourceKey))
                {
                    throw new InvalidOperationException("Lock resource key is null or empty.");
                }

                var result = await distributedLockService.ExecuteAsync(resourceKey,
                    () => CapturePaymentInternalAsync(request),
                    _lockOptions.LockTimeout,
                    _lockOptions.TryLockTimeout,
                    _lockOptions.RetryInterval);

                return result;
            }
            catch (Exception ex)
            {
                return new CaptureOrderPaymentResult
                {
                    Succeeded = false,
                    ErrorCode = PaymentFlowErrorCodes.PaymentFailed,
                    ErrorMessage = ex.Message
                };
            }
        }

        // Validate, add new capture request, save to DB
        public virtual async Task<CaptureOrderPaymentResult> CreateCaptureDocument(CaptureOrderPaymentRequest request)
        {
            var result = AbstractTypeFactory<CaptureOrderPaymentResult>.TryCreateInstance();

            // Paid status is also available for capture operation, since in case of multiple captures per single payment we don't know when it will be the last capture
            var paymentInfo = await GetPaymentInfoAsync(request, CaptureAllowedPaymentStatuses);

            // Validate payment
            var validationResult = await validator.ValidateAsync(paymentInfo, options => options.IncludeRuleSets(CaptureRuleSets));
            if (!validationResult.IsValid)
            {
                var error = validationResult.Errors.FirstOrDefault();
                result.ErrorMessage = error?.ErrorMessage;
                result.ErrorCode = error?.ErrorCode;

                return result;
            }

            paymentInfo.Payment.Captures ??= new List<Capture>();

            // Allows to Update and Retry Rejected Capture document.
            var capture = paymentInfo.Payment.Captures.FirstOrDefault(c => c.TransactionId == request.TransactionId
                && c.Status != nameof(CaptureStatus.Processed));

            if (capture != null)
            {
                UpdateCapture(capture, paymentInfo.Payment, paymentInfo.Store, request);
            }
            else
            {
                capture = CreateCapture(paymentInfo.Payment, paymentInfo.Store, request);
                paymentInfo.Payment.Captures.Add(capture);
            }

            await customerOrderService.SaveChangesAsync([paymentInfo.CustomerOrder]);

            result.Succeeded = true;

            return result;
        }

        public virtual async Task<CaptureOrderPaymentResult> SaveResultToCaptureDocument(CaptureOrderPaymentRequest request, CapturePaymentRequestResult captureResult)
        {
            var result = AbstractTypeFactory<CaptureOrderPaymentResult>.TryCreateInstance();

            var paymentInfo = await GetPaymentInfoAsync(request, CaptureAllowedPaymentStatuses);
            var capture = paymentInfo.Payment.Captures.First(c => c.TransactionId == request.TransactionId);

            if (captureResult.IsSuccess)
            {
                capture.Status = nameof(CaptureStatus.Processed);
                paymentInfo.Payment.Status = captureResult.NewPaymentStatus.ToString();
                result.PaymentStatus = paymentInfo.Payment.Status;
                result.Succeeded = true;
            }
            else
            {
                capture.Status = nameof(CaptureStatus.Rejected);
                result.ErrorCode = PaymentFlowErrorCodes.PaymentFailed;
                result.ErrorMessage = captureResult.ErrorMessage;
            }

            await customerOrderService.SaveChangesAsync([paymentInfo.CustomerOrder]);

            return result;
        }

        public virtual async Task<CapturePaymentRequestResult> ProcessCapture(CaptureOrderPaymentRequest request)
        {
            // Paid status is also available for capture operation, since in case of multiple captures per single payment we don't know when it will be the last capture
            var paymentInfo = await GetPaymentInfoAsync(request, CaptureAllowedPaymentStatuses);
            var captureRequest = GetCapturePaymentRequest(paymentInfo, request);

            return paymentInfo.Payment.PaymentMethod.CaptureProcessPayment(captureRequest);
        }

        protected virtual async Task<CaptureOrderPaymentResult> CapturePaymentInternalAsync(CaptureOrderPaymentRequest request)
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

        protected virtual CapturePaymentRequest GetCapturePaymentRequest(OrderPaymentInfo paymentInfo, CaptureOrderPaymentRequest request)
        {
            var result = AbstractTypeFactory<CapturePaymentRequest>.TryCreateInstance();

            FillPaymentRequestBase(paymentInfo, result);

            result.CaptureAmount = request.Amount ?? paymentInfo.Payment.Sum;
            result.OuterId = request.OuterId;

            result.Parameters ??= [];

            result.Parameters.Add(nameof(request.CloseTransaction), request.CloseTransaction.ToString());
            result.Parameters.Add(nameof(request.CaptureDetails), request.CaptureDetails ?? string.Empty);
            result.Parameters.Add(nameof(request.TransactionId), request.TransactionId ?? string.Empty);

            return result;
        }

        protected virtual Capture CreateCapture(PaymentIn payment, Store store, CaptureOrderPaymentRequest request)
        {
            var capture = AbstractTypeFactory<Capture>.TryCreateInstance();

            var numberTemplate = store.Settings.GetValue<string>(Core.ModuleConstants.Settings.General.CaptureNewNumberTemplate);
            capture.Number = uniqueNumberGenerator.GenerateNumber(store.Id, numberTemplate);

            capture.Amount = request.Amount ?? payment.Sum;
            capture.Comment = request.CaptureDetails;
            capture.OuterId = request.OuterId;
            capture.TransactionId = request.TransactionId;
            capture.CloseTransaction = request.CloseTransaction;

            capture.Status = nameof(CaptureStatus.Pending);
            capture.Currency = payment.Currency;
            capture.CustomerOrderId = payment.OrderId;
            capture.VendorId = payment.VendorId;

            return capture;
        }

        protected virtual void UpdateCapture(Capture capture, PaymentIn payment, Store store, CaptureOrderPaymentRequest request)
        {
            capture.Amount = request.Amount ?? payment.Sum;
            capture.Comment = request.CaptureDetails;
            capture.OuterId = request.OuterId;
            capture.TransactionId = request.TransactionId;
            capture.CloseTransaction = request.CloseTransaction;

            capture.Status = nameof(CaptureStatus.Pending);
            capture.Currency = payment.Currency;
            capture.CustomerOrderId = payment.OrderId;
            capture.VendorId = payment.VendorId;
        }

        #endregion

        protected virtual async Task<OrderPaymentInfo> GetPaymentInfoAsync(OrderPaymentRequest request, params PaymentStatus[] allowedStatuses)
        {
            var result = AbstractTypeFactory<OrderPaymentInfo>.TryCreateInstance();

            if (!string.IsNullOrEmpty(request.OrderId))
            {
                result.CustomerOrder = await customerOrderService.GetByIdAsync(request.OrderId, nameof(CustomerOrderResponseGroup.Full));

                result.Payment = string.IsNullOrEmpty(request.PaymentId)
                    ? result.CustomerOrder?.InPayments?.FirstOrDefault(x => allowedStatuses.Contains(x.PaymentStatus))
                    : result.CustomerOrder?.InPayments?.FirstOrDefault(x => x.Id == request.PaymentId && allowedStatuses.Contains(x.PaymentStatus));
            }
            else if (!string.IsNullOrEmpty(request.PaymentId))
            {
                result.Payment = await paymentService.GetByIdAsync(request.PaymentId);
                result.CustomerOrder = await customerOrderService.GetByIdAsync(result.Payment?.OrderId, nameof(CustomerOrderResponseGroup.Full));

                // take payment from order since Order payment contains instanced PaymentMethod (payment taken from service doesn't)
                result.Payment = result.CustomerOrder?.InPayments?.FirstOrDefault(x => x.Id == request.PaymentId && allowedStatuses.Contains(x.PaymentStatus));
            }

            result.Store = await storeService.GetByIdAsync(result.CustomerOrder?.StoreId, nameof(StoreResponseGroup.StoreInfo));

            return result;
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
