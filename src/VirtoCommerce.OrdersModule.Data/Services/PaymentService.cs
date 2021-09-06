using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.OrdersModule.Data.Services
{
    public class PaymentService : CrudService<PaymentIn, PaymentInEntity, PaymentChangeEvent, PaymentChangedEvent>, IPaymentService
    {
        private new readonly Func<IOrderRepository> _repositoryFactory;
        private readonly ICustomerOrderService _customerOrderService;
        private readonly ICrudService<CustomerOrder> _customerOrderServiceCrud;


        public PaymentService(
            Func<IOrderRepository> orderRepositoryFactory, IPlatformMemoryCache platformMemoryCache, IEventPublisher eventPublisher,
            ICustomerOrderService customerOrderService)
             : base(orderRepositoryFactory, platformMemoryCache, eventPublisher)
        {
            _repositoryFactory = orderRepositoryFactory;
            _customerOrderService = customerOrderService;
            _customerOrderServiceCrud = (ICrudService<CustomerOrder>)customerOrderService;
        }

        public virtual async Task<PaymentIn[]> GetByIdsAsync(string[] ids, string responseGroup = null)
        {
            var orders = await base.GetByIdsAsync(ids);
            return orders.ToArray();
        }

        public virtual Task SaveChangesAsync(PaymentIn[] payments)
        {
            if (payments == null)
            {
                throw new ArgumentNullException(nameof(payments));
            }

            return DoBulkActionsWithOrderAggregate(payments, (order, payment) =>
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
                var ordersAggregates = await _customerOrderServiceCrud.GetByIdsAsync(oderIds);
                foreach (var payment in payments)
                {
                    var orderAggregateRoot = ordersAggregates.FirstOrDefault(x => x.Id == payment.OrderId);
                    if (orderAggregateRoot != null)
                    {
                        orderAggregateRoot.InPayments.Remove(payment);
                        orderAggregateRoot.InPayments.Add(payment);
                    }
                }
                await _customerOrderService.SaveChangesAsync(ordersAggregates.ToArray());
            }
        }

        protected async override Task<IEnumerable<PaymentInEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return await ((IOrderRepository)repository).GetPaymentsByIdsAsync(ids.ToArray(), responseGroup);
        }
    }
}
