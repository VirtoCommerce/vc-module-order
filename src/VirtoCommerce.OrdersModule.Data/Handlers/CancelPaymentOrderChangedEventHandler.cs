using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.OrdersModule.Data.Handlers
{
    public class CancelPaymentOrderChangedEventHandler : IEventHandler<OrderChangedEvent>
    {
        private readonly ICustomerOrderService _orderService;

        public CancelPaymentOrderChangedEventHandler(ICustomerOrderService customerOrderService)
        {
            _orderService = customerOrderService;
        }

        public virtual Task Handle(OrderChangedEvent message)
        {
            var jobArguments = message.ChangedEntries.SelectMany(GetJobArgumentsForChangedEntry).ToArray();

            if (jobArguments.Any())
            {
                BackgroundJob.Enqueue(() => TryToCancelOrderPaymentsAsync(jobArguments));
            }
            return Task.CompletedTask;
        }

        public virtual async Task TryToCancelOrderPaymentsAsync(PaymentToCancelJobArgument[] jobArguments)
        {
            var ordersByIdDict = (await _orderService.GetAsync(jobArguments.Select(x => x.CustomerOrderId).Distinct().ToArray()))
                                .ToDictionary(x => x.Id).WithDefaultValue(null);
            var changedOrders = new List<CustomerOrder>();
            foreach (var jobArgument in jobArguments)
            {
                var order = ordersByIdDict[jobArgument.CustomerOrderId];
                if (order != null)
                {
                    var paymentToCancel = order.InPayments.FirstOrDefault(x => x.Id.EqualsIgnoreCase(jobArgument.PaymentId));
                    if (paymentToCancel != null && !paymentToCancel.IsCancelled)
                    {
#pragma warning disable VC0012
                        CancelPayment(paymentToCancel, order);
#pragma warning restore VC0012

                        if (!changedOrders.Contains(order))
                        {
                            changedOrders.Add(order);
                        }
                    }
                }
            }
            if (changedOrders.Any())
            {
                await _orderService.SaveChangesAsync(changedOrders.ToArray());
            }
        }


        protected virtual PaymentToCancelJobArgument[] GetJobArgumentsForChangedEntry(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var toCancelPayments = new List<PaymentIn>();
            var isOrderCancelled = !changedEntry.OldEntry.IsCancelled && changedEntry.NewEntry.IsCancelled;
            if (isOrderCancelled)
            {
                toCancelPayments = changedEntry.NewEntry.InPayments?.ToList();
            }
            else
            {
                foreach (var canceledPayment in changedEntry.NewEntry?.InPayments.Where(x => x.CancelledState == CancelledState.Requested))
                {
                    var oldSamePayment = changedEntry.OldEntry?.InPayments.FirstOrDefault(x => x == canceledPayment);
                    if (oldSamePayment != null && oldSamePayment.CancelledState != CancelledState.Completed)
                    {
                        toCancelPayments.Add(canceledPayment);
                    }
                }
            }

            return toCancelPayments.Select(x => PaymentToCancelJobArgument.FromChangedEntry(changedEntry, x)).ToArray();
        }

        [Obsolete("Use CancelPaymentAsync method instead", DiagnosticId = "VC0012", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        protected virtual void CancelPayment(PaymentIn paymentToCancel, CustomerOrder order)
        {
            CancelPaymentAsync(paymentToCancel, order).GetAwaiter().GetResult();
        }

        protected virtual async Task CancelPaymentAsync(PaymentIn paymentToCancel, CustomerOrder order)
        {
            if (paymentToCancel.PaymentStatus == PaymentStatus.Authorized)
            {
                if (paymentToCancel.PaymentMethod != null)
                {
                    await paymentToCancel.PaymentMethod.VoidProcessPaymentAsync(new VoidPaymentRequest { PaymentId = paymentToCancel.Id, OrderId = order.Id, Payment = paymentToCancel, Order = order });
                }
            }
            else if (paymentToCancel.PaymentStatus == PaymentStatus.Paid)
            {
                if (paymentToCancel.PaymentMethod != null)
                {
                    await paymentToCancel.PaymentMethod.RefundProcessPaymentAsync(new RefundPaymentRequest { PaymentId = paymentToCancel.Id, OrderId = order.Id, Payment = paymentToCancel, Order = order });
                }
            }
            else
            {
                paymentToCancel.PaymentStatus = PaymentStatus.Cancelled;
                paymentToCancel.IsCancelled = true;
                paymentToCancel.CancelledDate = DateTime.UtcNow;
            }

            paymentToCancel.Status = paymentToCancel.PaymentStatus.ToString();
            paymentToCancel.CancelledState = CancelledState.Completed;
        }
    }

    public class PaymentToCancelJobArgument
    {
        public string CustomerOrderId { get; set; }
        public string PaymentId { get; set; }
        public string StoreId { get; set; }

        public static PaymentToCancelJobArgument FromChangedEntry(GenericChangedEntry<CustomerOrder> changedEntry, PaymentIn payment)
        {
            var result = new PaymentToCancelJobArgument
            {
                CustomerOrderId = changedEntry?.OldEntry.Id,
                PaymentId = payment?.Id,
                StoreId = changedEntry?.NewEntry.StoreId
            };
            return result;
        }
    }
}
