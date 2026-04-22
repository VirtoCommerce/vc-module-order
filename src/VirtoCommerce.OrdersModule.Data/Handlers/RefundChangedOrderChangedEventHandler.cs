using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.OrdersModule.Data.Handlers
{
    public class RefundChangedOrderChangedEventHandler : IEventHandler<OrderChangedEvent>
    {
        private readonly ICustomerOrderService _orderService;
        private readonly IStoreService _storeService;

        public RefundChangedOrderChangedEventHandler(
            ICustomerOrderService orderService,
            IStoreService storeService)
        {
            _orderService = orderService;
            _storeService = storeService;
        }

        public virtual Task Handle(OrderChangedEvent message)
        {
            var jobArguments = message.ChangedEntries
                .Where(x => x.EntryState == EntryState.Modified)
                .SelectMany(GetJobArgumentsForChangedEntry)
                .ToArray();

            if (jobArguments.Length > 0)
            {
                BackgroundJob.Enqueue(() => ProcessRefundChangesAsync(jobArguments));
            }

            return Task.CompletedTask;
        }

        public virtual async Task ProcessRefundChangesAsync(RefundChangedJobArgument[] jobArguments)
        {
            var orderIds = jobArguments.Select(x => x.CustomerOrderId).Distinct().ToArray();
            var ordersByIdDict = (await _orderService.GetAsync(orderIds))
                .ToDictionary(x => x.Id)
                .WithDefaultValue(null);

            var changedOrders = new List<CustomerOrder>();

            foreach (var arg in jobArguments)
            {
                var order = ordersByIdDict[arg.CustomerOrderId];
                if (order == null)
                {
                    continue;
                }

                var payment = order.InPayments?.FirstOrDefault(p => p.Id.EqualsIgnoreCase(arg.PaymentId));
                if (payment?.PaymentMethod == null || payment.PaymentMethod is not ISupportRefundFlow)
                {
                    continue;
                }

                var refund = payment.Refunds?.FirstOrDefault(r => r.Id.EqualsIgnoreCase(arg.RefundId));
                if (refund == null || refund.Status == nameof(RefundStatus.Processed))
                {
                    continue;
                }

                var store = await _storeService.GetByIdAsync(order.StoreId, nameof(StoreResponseGroup.StoreInfo));

                var refundRequest = AbstractTypeFactory<RefundPaymentRequest>.TryCreateInstance();
                refundRequest.PaymentId = payment.Id;
                refundRequest.Payment = payment;
                refundRequest.OrderId = order.Id;
                refundRequest.Order = order;
                refundRequest.StoreId = store?.Id;
                refundRequest.Store = store;
                refundRequest.AmountToRefund = refund.Amount;
                refundRequest.Reason = refund.ReasonCode.ToString();
                refundRequest.Notes = refund.ReasonMessage;
                refundRequest.OuterId = refund.OuterId;
                refundRequest.Parameters = new NameValueCollection
                {
                    { nameof(refund.TransactionId), refund.TransactionId ?? string.Empty },
                    { "IsUpdate", "true" },
                };

                var previousStatus = refund.Status;
                try
                {
                    var result = await payment.PaymentMethod.RefundProcessPaymentAsync(refundRequest);
                    if (result.IsSuccess)
                    {
                        refund.Status = result.NewRefundStatus.ToString();
                        refund.RejectReasonMessage = null;
                    }
                    else
                    {
                        // A failed modification must not invalidate the underlying refund state.
                        // NewRefundStatus is non-nullable and defaults to Pending, so we cannot distinguish
                        // "provider didn't set it" from "provider explicitly reports Pending" -- preserve prior state.
                        refund.Status = previousStatus;
                        refund.RejectReasonMessage = result.ErrorMessage;
                    }

                    if (!changedOrders.Contains(order))
                    {
                        changedOrders.Add(order);
                    }
                }
                catch (Exception)
                {
                    // Log error but don't throw -- this is a background job
                }
            }

            if (changedOrders.Count > 0)
            {
                await _orderService.SaveChangesAsync(changedOrders.ToArray());
            }
        }

        protected virtual RefundChangedJobArgument[] GetJobArgumentsForChangedEntry(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var result = new List<RefundChangedJobArgument>();

            var newPayments = changedEntry.NewEntry.InPayments ?? new List<PaymentIn>();
            var oldPayments = changedEntry.OldEntry.InPayments ?? new List<PaymentIn>();

            foreach (var newPayment in newPayments)
            {
                var oldPayment = oldPayments.FirstOrDefault(p => p.Id.EqualsIgnoreCase(newPayment.Id));
                if (oldPayment == null)
                {
                    continue;
                }

                var newRefunds = newPayment.Refunds ?? new List<Refund>();
                var oldRefunds = oldPayment.Refunds ?? new List<Refund>();

                foreach (var newRefund in newRefunds)
                {
                    var oldRefund = oldRefunds.FirstOrDefault(r => r.Id.EqualsIgnoreCase(newRefund.Id));
                    // Skip newly added refunds - they are handled by PaymentFlowService
                    if (oldRefund == null)
                    {
                        continue;
                    }

                    if (HasRefundFieldChanges(oldRefund, newRefund))
                    {
                        result.Add(new RefundChangedJobArgument
                        {
                            CustomerOrderId = changedEntry.NewEntry.Id,
                            PaymentId = newPayment.Id,
                            RefundId = newRefund.Id,
                        });
                    }
                }
            }

            return result.ToArray();
        }

        protected virtual bool HasRefundFieldChanges(Refund oldRefund, Refund newRefund)
        {
            // Status and OuterId are intentionally NOT tracked here: both are written by the provider response
            // (via PaymentFlowService.SaveResultToRefundDocument or this handler), so tracking them would
            // re-trigger the handler from its own SaveChangesAsync and cause duplicate provider calls.
            // Manual status edits on submitted refunds are blocked at the UI.
            return oldRefund.Amount != newRefund.Amount
                || oldRefund.ReasonCode != newRefund.ReasonCode
                || oldRefund.ReasonMessage != newRefund.ReasonMessage
                || oldRefund.IsCancelled != newRefund.IsCancelled;
        }
    }

    public class RefundChangedJobArgument
    {
        public string CustomerOrderId { get; set; }
        public string PaymentId { get; set; }
        public string RefundId { get; set; }
    }
}
