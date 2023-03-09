using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Data.Services
{
    public class PaymentFlowService : IPaymentFlowService
    {
        private readonly ICrudService<CustomerOrder> _customerOrderService;
        private readonly ICrudService<PaymentIn> _paymentService;
        private readonly ICrudService<Store> _storeService;
        private readonly IValidator<OrderPaymentInfo> _validator;

        public PaymentFlowService(
            ICrudService<CustomerOrder> customerOrderService,
            ICrudService<PaymentIn> paymentService,
            ICrudService<Store> storeService,
            IValidator<OrderPaymentInfo> validator)
        {
            _customerOrderService = customerOrderService;
            _paymentService = paymentService;
            _storeService = storeService;
            _validator = validator;
        }

        public virtual async Task<OrderPaymentResult> CapturePaymentAsync(OrderPaymentRequest request)
        {
            var result = AbstractTypeFactory<OrderPaymentResult>.TryCreateInstance();

            var paymentInfo = await GetPaymentInfoAsync(request);

            // validate payment
            var validationResult = _validator.Validate(paymentInfo);
            if (!validationResult.IsValid)
            {
                result.ErrorMessage = validationResult.Errors.FirstOrDefault().ErrorMessage;
                return result;
            }

            var captureRequest = GetCapturePaymentRequest(paymentInfo, request);

            try
            {
                var captureResult = paymentInfo.Payment.PaymentMethod.CaptureProcessPayment(captureRequest);

                // save order
                if (captureResult.IsSuccess)
                {
                    paymentInfo.Payment.Status = captureResult.NewPaymentStatus.ToString();

                    await _customerOrderService.SaveChangesAsync(new[] { paymentInfo.CustomerOrder });

                    result.IsSuccess = true;
                }
                else
                {
                    result.ErrorMessage = captureResult.ErrorMessage;
                }
            }
            catch (Exception)
            {
                result.ErrorMessage = PaymentErrorDescriber.PaymentMethodError();
            }

            return result;
        }

        protected virtual async Task<OrderPaymentInfo> GetPaymentInfoAsync(OrderPaymentRequest request)
        {
            var result = AbstractTypeFactory<OrderPaymentInfo>.TryCreateInstance();

            if (!string.IsNullOrEmpty(request.OrderId))
            {
                result.CustomerOrder = await _customerOrderService.GetByIdAsync(request.OrderId, CustomerOrderResponseGroup.Full.ToString());

                result.Payment = string.IsNullOrEmpty(request.PaymentId)
                    ? result.CustomerOrder?.InPayments?.FirstOrDefault(x => x.Status == PaymentStatus.Authorized.ToString())
                    : result.CustomerOrder?.InPayments?.FirstOrDefault(x => x.Id == request.PaymentId && x.Status == PaymentStatus.Authorized.ToString());
            }
            else if (!string.IsNullOrEmpty(request.PaymentId))
            {
                result.Payment = await _paymentService.GetByIdAsync(request.PaymentId);
                result.CustomerOrder = await _customerOrderService.GetByIdAsync(result.Payment?.OrderId, CustomerOrderResponseGroup.Full.ToString());

                // take payment from order since Order payment contains instanced PaymentMethod (payment taken from service doesn't)
                result.Payment = result.CustomerOrder?.InPayments?.FirstOrDefault(x => x.Id == request.PaymentId && x.Status == PaymentStatus.Authorized.ToString());
            }

            result.Store = await _storeService.GetByIdAsync(result.CustomerOrder?.StoreId, StoreResponseGroup.StoreInfo.ToString());

            return result;
        }

        protected virtual CapturePaymentRequest GetCapturePaymentRequest(OrderPaymentInfo paymentInfo, OrderPaymentRequest request)
        {
            var result = AbstractTypeFactory<CapturePaymentRequest>.TryCreateInstance();

            result.OrderId = paymentInfo.CustomerOrder.Id;
            result.Order = paymentInfo.CustomerOrder;
            result.PaymentId = paymentInfo.Payment.Id;
            result.Payment = paymentInfo.Payment;
            result.StoreId = paymentInfo.Store.Id;
            result.Store = paymentInfo.Store;

            result.CaptureAmount = request.Amount;

            return result;
        }
    }
}
