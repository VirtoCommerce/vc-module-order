using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.OrdersModule.Data.Services
{
    public class PaymentService : CrudService<PaymentIn, PaymentInEntity, PaymentChangeEvent, PaymentChangedEvent>, IPaymentService
    {
        private readonly Func<IOrderRepository> _repositoryFactory;
        private readonly ICustomerOrderService _customerOrderService;

        public PaymentService(
            Func<IOrderRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher,
            ICustomerOrderService customerOrderService)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _repositoryFactory = repositoryFactory;
            _customerOrderService = customerOrderService;
        }

        public override Task SaveChangesAsync(IList<PaymentIn> models)
        {
            if (models == null)
            {
                throw new ArgumentNullException(nameof(models));
            }

            return DoBulkActionsWithOrderAggregate(models, (order, payment) =>
            {
                order.InPayments.Remove(payment);
                order.InPayments.Add(payment);
            });
        }

        public override async Task DeleteAsync(IList<string> ids, bool softDelete = false)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            using var repository = _repositoryFactory();
            var paymentEntities = await repository.GetPaymentsByIdsAsync(ids);
            var payments = paymentEntities.Select(ToModel).ToArray();

            await DoBulkActionsWithOrderAggregate(payments, (order, payment) =>
            {
                order.InPayments.Remove(payment);
            });
        }

        protected virtual async Task DoBulkActionsWithOrderAggregate(IList<PaymentIn> payments, Action<CustomerOrder, PaymentIn> action)
        {
            if (payments.Any(x => string.IsNullOrEmpty(x.OrderId)))
            {
                throw new OperationCanceledException($"{nameof(PaymentIn.OrderId)} must be set.");
            }
            var oderIds = payments.Select(x => x.OrderId).Distinct().ToArray();
            if (oderIds.Any())
            {
                var ordersAggregates = await _customerOrderService.GetAsync(oderIds);
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

        protected override Task<IList<PaymentInEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
        {
            return ((IOrderRepository)repository).GetPaymentsByIdsAsync(ids);
        }

        protected override void ClearCache(IList<PaymentIn> models)
        {
            base.ClearCache(models);

            // Clear order cache
            GenericSearchCachingRegion<CustomerOrder>.ExpireRegion();

            var orderIds = models.Select(x => x.OrderId).Distinct().ToArray();

            foreach (var id in orderIds)
            {
                GenericCachingRegion<CustomerOrder>.ExpireTokenForKey(id);
            }
        }
    }
}
