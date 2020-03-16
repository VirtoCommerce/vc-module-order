using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.OrdersModule.Data.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly Func<IOrderRepository> _repositoryFactory;
        private readonly ICustomerOrderService _customerOrderService;


        public PaymentService(
            Func<IOrderRepository> orderRepositoryFactory,
            ICustomerOrderService customerOrderService)
        {
            _repositoryFactory = orderRepositoryFactory;
            _customerOrderService = customerOrderService;
        }

        public virtual async Task<PaymentIn[]> GetByIdsAsync(string[] ids, string responseGroup = null)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            var result = new List<PaymentIn>();
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                var paymentEntities = await repository.GetPaymentsByIdsAsync(ids, responseGroup);
                foreach (var paymentEntity in paymentEntities)
                {
                    var payment = paymentEntity.ToModel(AbstractTypeFactory<PaymentIn>.TryCreateInstance()) as PaymentIn;

                    result.Add(payment);
                }
            }
            return result.ToArray();
        }

        public virtual async Task<PaymentIn> GetByIdAsync(string ids, string responseGroup = null)
        {
            var orders = await GetByIdsAsync(new[] { ids }, responseGroup);
            return orders.FirstOrDefault();
        }

        public virtual async Task SaveChangesAsync(PaymentIn[] payments)
        {
            if (payments == null)
            {
                throw new ArgumentNullException(nameof(payments));
            }

            await DoBulkActionsWithOrderAggregate(payments, (order, payment) =>
            {
                order.InPayments.Remove(payment);
                order.InPayments.Add(payment);
            });
        }

        public virtual async Task DeleteAsync(string[] ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }
            using (var repository = _repositoryFactory())
            {
                var paymentEntities = await repository.GetPaymentsByIdsAsync(ids);
                var payments = paymentEntities.Select(x => x.ToModel(AbstractTypeFactory<PaymentIn>.TryCreateInstance())).OfType<PaymentIn>().ToArray();
                await DoBulkActionsWithOrderAggregate(payments, (order, payment) =>
               {
                   order.InPayments.Remove(payment);
               });
            }
        }

        protected virtual async Task DoBulkActionsWithOrderAggregate(PaymentIn[] payments, Action<CustomerOrder, PaymentIn> action)
        {
            if (payments.Any(x => string.IsNullOrEmpty(x.OrderId)))
            {
                throw new OperationCanceledException($"{ nameof(PaymentIn.OrderId) } must be set.");
            }
            var oderIds = payments.Select(x => x.OrderId).Distinct().ToArray();
            if (oderIds.Any())
            {
                var ordersAggregates = await _customerOrderService.GetByIdsAsync(oderIds);
                foreach (var payment in payments)
                {
                    var orderAggregateRoot = ordersAggregates.FirstOrDefault(x => x.Id == payment.OrderId);
                    if (orderAggregateRoot != null)
                    {
                        orderAggregateRoot.InPayments.Remove(payment);
                        orderAggregateRoot.InPayments.Add(payment);
                    }
                }
                await _customerOrderService.SaveChangesAsync(ordersAggregates);
            }
        }
    }
}
