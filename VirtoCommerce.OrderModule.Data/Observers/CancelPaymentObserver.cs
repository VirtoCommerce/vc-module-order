using System;
using System.Linq;
using VirtoCommerce.Domain.Order.Events;
using VirtoCommerce.Domain.Payment.Model;
using VirtoCommerce.Domain.Store.Services;

namespace VirtoCommerce.OrderModule.Data.Observers
{
    public class CancelPaymentObserver : IObserver<OrderChangedEvent>
    {
        private readonly IStoreService _storeService;

        public CancelPaymentObserver(IStoreService storeService)
        {
            _storeService = storeService;
        }

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {

        }

        public void OnNext(OrderChangedEvent value)
        {
            CancelationPayment(value);
        }

        private void CancelationPayment(OrderChangedEvent value)
        {
            if (value.OrigOrder != null && !value.OrigOrder.IsCancelled && value.ModifiedOrder.IsCancelled)
            {
                foreach (var payment in value.ModifiedOrder.InPayments)
                {
                    var store = _storeService.GetById(value.ModifiedOrder.StoreId);
                    var method = store.PaymentMethods.FirstOrDefault(p => p.Code == payment.GatewayCode);

                    if (!payment.IsCancelled && payment.PaymentStatus == PaymentStatus.Authorized)
                    {
                        method?.VoidProcessPayment(new VoidProcessPaymentEvaluationContext { Payment = payment });
                    }
                    else if (!payment.IsCancelled && payment.PaymentStatus == PaymentStatus.Paid)
                    {
                        method?.RefundProcessPayment(new RefundProcessPaymentEvaluationContext { Payment = payment });
                    }
                    else
                    {
                        payment.PaymentStatus = PaymentStatus.Cancelled;
                        payment.IsCancelled = true;
                        payment.CancelledDate = DateTime.UtcNow;
                    }
                }
            }

            if (value.OrigOrder != null && value.OrigOrder.InPayments != null && value.OrigOrder.InPayments.Any() &&
                value.ModifiedOrder.InPayments != null && value.ModifiedOrder.InPayments.Any(p => p.PaymentStatus == PaymentStatus.Cancelled)
                )
                foreach (var payment in value.ModifiedOrder.InPayments.Where(p => p.PaymentStatus == PaymentStatus.Cancelled))
                {
                    var op = value.OrigOrder.InPayments.FirstOrDefault(p => p.Id == payment.Id);
                    if (op != null && op.PaymentStatus != PaymentStatus.Cancelled && op.PaymentStatus != PaymentStatus.Voided && op.PaymentStatus != PaymentStatus.Refunded)
                    {
                        var store = _storeService.GetById(value.ModifiedOrder.StoreId);
                        var method = store.PaymentMethods.FirstOrDefault(p => p.Code == op.GatewayCode);
                        if (method != null)
                        {
                            if (op.PaymentStatus == PaymentStatus.Authorized)
                            {
                                method.VoidProcessPayment(new VoidProcessPaymentEvaluationContext { Payment = payment });
                            }
                            else if (op.PaymentStatus == PaymentStatus.Paid)
                            {
                                method.RefundProcessPayment(new RefundProcessPaymentEvaluationContext { Payment = payment });
                            }
                        }
                    }
                }
        }
    }
}
