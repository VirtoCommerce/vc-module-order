using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Core.Model.Search;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.OrdersModule.Data.Handlers
{
    public class CancelPaymentOrderChangedEventHandler : IEventHandler<OrderChangedEvent>
    {
        private readonly IStoreService _storeService;
        private readonly ICustomerOrderService _orderService;
        private readonly IPaymentMethodsSearchService _paymentMethodsSearchService;

        public CancelPaymentOrderChangedEventHandler(
            IStoreService storeService,
            ICustomerOrderService customerOrderService,
            IPaymentMethodsSearchService paymentMethodsSearchService
            )
        {
            _storeService = storeService;
            _orderService = customerOrderService;
            _paymentMethodsSearchService = paymentMethodsSearchService;
        }


        public virtual Task Handle(OrderChangedEvent @event)
        {
            if (@event.ChangedEntries.Any())
            { 
                BackgroundJob.Enqueue(() => TryToCancelOrderBackgroundJob(@event));
            }
            return Task.CompletedTask;
        }

        [DisableConcurrentExecution(60 * 60 * 24)]
        public async Task TryToCancelOrderBackgroundJob(OrderChangedEvent @event)
        {        
            foreach (var changedEntry in @event.ChangedEntries.Where(x => x.EntryState == EntryState.Modified))
            {
                await TryToCancelOrders(changedEntry.NewEntry, changedEntry.OldEntry);
            }
        }

        protected virtual async Task TryToCancelOrders(CustomerOrder changedOrder, CustomerOrder oldOrder)
        {
            var store = await _storeService.GetByIdAsync(changedOrder.StoreId, StoreResponseGroup.StoreInfo.ToString());

            //Try to load payment methods for payments
            var gatewayCodes = changedOrder.InPayments.Select(x => x.GatewayCode).ToArray();
            var paymentMethods = await GetPaymentMethodsAsync(store.Id, gatewayCodes);
            foreach (var payment in changedOrder.InPayments)
            {
                payment.PaymentMethod = paymentMethods.FirstOrDefault(x => x.Code == payment.GatewayCode);
            }

            var toCancelPayments = new List<PaymentIn>();
            var isOrderCancelled = !oldOrder.IsCancelled && changedOrder.IsCancelled;
            if (isOrderCancelled)
            {
                toCancelPayments = changedOrder.InPayments?.ToList();
            }
            else
            {
                foreach (var canceledPayment in changedOrder?.InPayments.Where(x => x.IsCancelled))
                {
                    var oldSamePayment = oldOrder?.InPayments.FirstOrDefault(x => x == canceledPayment);
                    if (oldSamePayment != null && !oldSamePayment.IsCancelled)
                    {
                        toCancelPayments.Add(canceledPayment);
                    }
                }
            }
            TryToCancelOrderPayments(toCancelPayments, changedOrder);
            if (!toCancelPayments.IsNullOrEmpty())
            {
                await _orderService.SaveChangesAsync(new[] { changedOrder });
            }
        }

        protected virtual void TryToCancelOrderPayments(IEnumerable<PaymentIn> toCancelPayments, CustomerOrder order)
        {
            foreach (var payment in toCancelPayments ?? Enumerable.Empty<PaymentIn>())
            {
                if (payment.PaymentStatus == PaymentStatus.Authorized)
                {
                    payment.PaymentMethod?.VoidProcessPayment(new VoidPaymentRequest { PaymentId = payment.Id, OrderId = order.Id });
                }
                else if (payment.PaymentStatus == PaymentStatus.Paid)
                {
                    payment.PaymentMethod?.RefundProcessPayment(new RefundPaymentRequest { PaymentId = payment.Id, OrderId = order.Id });
                }
                else
                {
                    payment.PaymentStatus = PaymentStatus.Cancelled;
                    payment.IsCancelled = true;
                    payment.CancelledDate = DateTime.UtcNow;
                }
            }
        }

        protected virtual async Task<ICollection<PaymentMethod>> GetPaymentMethodsAsync(string storeId, string[] codes)
        {
            var criteria = new PaymentMethodsSearchCriteria
            {
                IsActive = true,
                StoreId = storeId,
                Codes = codes,
                Take = int.MaxValue
            };

            var searchResult = await _paymentMethodsSearchService.SearchPaymentMethodsAsync(criteria);
            var paymentMethod = searchResult.Results;

            return paymentMethod;
        }
    }
}
