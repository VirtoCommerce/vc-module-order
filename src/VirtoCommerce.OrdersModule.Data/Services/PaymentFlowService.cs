using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Validators;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
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

        private readonly PaymentStatus _captureAllowedPaymentStatus = PaymentStatus.Authorized;
        private readonly PaymentStatus _refundAllowedPaymentStatus = PaymentStatus.Paid;

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

            var paymentInfo = await GetPaymentInfoAsync(request, _refundAllowedPaymentStatus);

            // validate payment
            var validationResult = _validator.Validate(paymentInfo, options => options.IncludeRuleSets(RefundRuleSets));
            if (!validationResult.IsValid)
            {
                var error = validationResult.Errors.FirstOrDefault();
                result.ErrorMessage = error?.ErrorMessage;
                result.ErrorCode = error?.ErrorCode;

                return result;
            }

            // create and save refund
            var refund = CreateRefund(paymentInfo.Payment, paymentInfo.Store, request);

            if (paymentInfo.Payment.Refunds == null)
            {
                paymentInfo.Payment.Refunds = new List<Refund>();
            }
            paymentInfo.Payment.Refunds.Add(refund);

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

            if (refundResult.IsSuccess)
            {
                refund.OuterId = refundResult.OuterId;
                refund.Status = refundResult.NewRefundStatus.ToString();
                await _customerOrderService.SaveChangesAsync(new[] { paymentInfo.CustomerOrder });

                result.RefundStatus = refund.Status;
                result.Succeeded = true;
            }
            else
            {
                result.ErrorCode = PaymentFlowErrorCodes.PaymentFailed;
                result.ErrorMessage = refundResult.ErrorMessage;
            }

            return result;
        }



        public virtual async Task<CaptureOrderPaymentResult> CapturePaymentAsync(CaptureOrderPaymentRequest request)
        {
            var result = AbstractTypeFactory<CaptureOrderPaymentResult>.TryCreateInstance();

            var paymentInfo = await GetPaymentInfoAsync(request, _captureAllowedPaymentStatus);

            // validate payment
            var validationResult = _validator.Validate(paymentInfo, options => options.IncludeRuleSets(CaptureRuleSets));
            if (!validationResult.IsValid)
            {
                var error = validationResult.Errors.FirstOrDefault();
                result.ErrorMessage = error?.ErrorMessage;
                result.ErrorCode = error?.ErrorCode;

                return result;
            }

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

            // save order
            if (captureResult.IsSuccess)
            {
                paymentInfo.Payment.Status = captureResult.NewPaymentStatus.ToString();
                await _customerOrderService.SaveChangesAsync(new[] { paymentInfo.CustomerOrder });

                result.PaymentStatus = paymentInfo.Payment.Status;
                result.Succeeded = true;
            }
            else
            {
                result.ErrorCode = PaymentFlowErrorCodes.PaymentFailed;
                result.ErrorMessage = captureResult.ErrorMessage;
            }

            return result;
        }

        protected virtual async Task<OrderPaymentInfo> GetPaymentInfoAsync(OrderPaymentRequest request, PaymentStatus paymentStatus)
        {
            var result = AbstractTypeFactory<OrderPaymentInfo>.TryCreateInstance();

            if (!string.IsNullOrEmpty(request.OrderId))
            {
                result.CustomerOrder = await _customerOrderService.GetByIdAsync(request.OrderId, CustomerOrderResponseGroup.Full.ToString());

                result.Payment = string.IsNullOrEmpty(request.PaymentId)
                    ? result.CustomerOrder?.InPayments?.FirstOrDefault(x => x.Status == paymentStatus.ToString())
                    : result.CustomerOrder?.InPayments?.FirstOrDefault(x => x.Id == request.PaymentId && x.Status == paymentStatus.ToString());
            }
            else if (!string.IsNullOrEmpty(request.PaymentId))
            {
                result.Payment = await _paymentService.GetByIdAsync(request.PaymentId);
                result.CustomerOrder = await _customerOrderService.GetByIdAsync(result.Payment?.OrderId, CustomerOrderResponseGroup.Full.ToString());

                // take payment from order since Order payment contains instanced PaymentMethod (payment taken from service doesn't)
                result.Payment = result.CustomerOrder?.InPayments?.FirstOrDefault(x => x.Id == request.PaymentId && x.Status == paymentStatus.ToString());
            }

            result.Store = await _storeService.GetByIdAsync(result.CustomerOrder?.StoreId, StoreResponseGroup.StoreInfo.ToString());

            return result;
        }

        protected virtual CapturePaymentRequest GetCapturePaymentRequest(OrderPaymentInfo paymentInfo, CaptureOrderPaymentRequest request)
        {
            var result = AbstractTypeFactory<CapturePaymentRequest>.TryCreateInstance();

            FillPaymentRequestBase(paymentInfo, result);

            result.CaptureAmount = request.Amount ?? paymentInfo.Payment.Sum;

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

            refund.Status = RefundStatus.Pending.ToString();
            refund.Currency = payment.Currency;
            refund.CustomerOrderId = payment.OrderId;
            refund.VendorId = payment.VendorId;

            return refund;
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
