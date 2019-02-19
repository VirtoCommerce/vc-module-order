using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Domain.Order.Events;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Domain.Order.Services;
using VirtoCommerce.Domain.Payment.Model;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.OrderModule.Data.Handlers
{
    public class CancelPaymentOrderChangedEventHandler : IEventHandler<OrderChangedEvent>
    {
        private readonly IStoreService _storeService;
        private readonly ICustomerOrderService _orderService;

        public CancelPaymentOrderChangedEventHandler(IStoreService storeService, ICustomerOrderService customerOrderService)
        {
            _storeService = storeService;
            _orderService = customerOrderService;
        }


        public virtual Task Handle(OrderChangedEvent message)
        {
            foreach (var changedEntry in message.ChangedEntries.Where(x => x.EntryState == EntryState.Modified))
            {
                TryToCancelOrder(changedEntry);
            }
            return Task.CompletedTask;
        }

        protected virtual void TryToCancelOrder(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var store = _storeService.GetById(changedEntry.NewEntry.StoreId);
            //Try to load payment methods for payments
            foreach (var payment in changedEntry.NewEntry?.InPayments ?? Enumerable.Empty<PaymentIn>())
            {
                payment.PaymentMethod = store.PaymentMethods.FirstOrDefault(p => p.Code.EqualsInvariant(payment.GatewayCode));
            }

            var toCancelPayments = new List<PaymentIn>();
            var isOrderCancelled = !changedEntry.OldEntry.IsCancelled && changedEntry.NewEntry.IsCancelled;
            if (isOrderCancelled)
            {
                toCancelPayments = changedEntry.NewEntry.InPayments?.ToList();
            }
            else
            {
                foreach (var canceledPayment in changedEntry.NewEntry?.InPayments?.Where(x => x.IsCancelled) ?? Enumerable.Empty<PaymentIn>())
                {
                    var oldSamePayment = changedEntry.OldEntry?.InPayments.FirstOrDefault(x => x == canceledPayment);
                    if (oldSamePayment != null && !oldSamePayment.IsCancelled)
                    {
                        toCancelPayments.Add(canceledPayment);
                    }
                }
            }
            TryToCancelOrderPayments(toCancelPayments,changedEntry.NewEntry);
            if (!toCancelPayments.IsNullOrEmpty())
            {
                _orderService.SaveChanges(new[] { changedEntry.NewEntry });
            }
        }


        protected virtual void TryToCancelOrderPayments(IEnumerable<PaymentIn> toCancelPayments, CustomerOrder order)
        {
            foreach (var payment in toCancelPayments ?? Enumerable.Empty<PaymentIn>())
            {
                if (payment.PaymentStatus == PaymentStatus.Authorized)
                {
                    payment.PaymentMethod?.VoidProcessPayment(new VoidProcessPaymentEvaluationContext { Payment = payment, Order = order });
                }
                else if (payment.PaymentStatus == PaymentStatus.Paid)
                {
                    payment.PaymentMethod?.RefundProcessPayment(new RefundProcessPaymentEvaluationContext { Payment = payment, Order = order });
                }
                else
                {
                    payment.PaymentStatus = PaymentStatus.Cancelled;
                    payment.IsCancelled = true;
                    payment.CancelledDate = DateTime.UtcNow;
                }
            }
        }
    }
}
