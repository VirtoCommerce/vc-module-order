using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Common;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Domain.Order.Events;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Domain.Order.Services;
using VirtoCommerce.Domain.Payment.Services;
using VirtoCommerce.Domain.Shipping.Services;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.OrderModule.Data.Model;
using VirtoCommerce.OrderModule.Data.Repositories;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.OrderModule.Data.Services
{
    public class CustomerOrderServiceImpl : ServiceBase, ICustomerOrderService, ICustomerOrderSearchService
    {

        public CustomerOrderServiceImpl(Func<IOrderRepository> orderRepositoryFactory, IUniqueNumberGenerator uniqueNumberGenerator, IDynamicPropertyService dynamicPropertyService, IShippingMethodsService shippingMethodsService, IPaymentMethodsService paymentMethodsService,
                                       IStoreService storeService, IChangeLogService changeLogService, IEventPublisher eventPublisher, ICustomerOrderTotalsCalculator totalsCalculator)
        {
            RepositoryFactory = orderRepositoryFactory;
            UniqueNumberGenerator = uniqueNumberGenerator;
            EventPublisher = eventPublisher;
            DynamicPropertyService = dynamicPropertyService;
            ShippingMethodsService = shippingMethodsService;
            PaymentMethodsService = paymentMethodsService;
            StoreService = storeService;
            ChangeLogService = changeLogService;
            TotalsCalculator = totalsCalculator;
        }

        protected IUniqueNumberGenerator UniqueNumberGenerator { get; }
        protected Func<IOrderRepository> RepositoryFactory { get; }
        protected IEventPublisher EventPublisher { get; }
        protected IDynamicPropertyService DynamicPropertyService { get; }
        protected IStoreService StoreService { get; }
        protected IPaymentMethodsService PaymentMethodsService { get; }
        protected IShippingMethodsService ShippingMethodsService { get; }
        protected IChangeLogService ChangeLogService { get; }
        protected ICustomerOrderTotalsCalculator TotalsCalculator;

        #region ICustomerOrderService Members

        public virtual void SaveChanges(CustomerOrder[] orders)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            var changedEntries = new List<GenericChangedEntry<CustomerOrder>>();

            using (var repository = RepositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                var dataExistOrders = repository.GetCustomerOrdersByIds(orders.Where(x => !x.IsTransient()).Select(x => x.Id).ToArray(), CustomerOrderResponseGroup.Full);
                foreach (var order in orders)
                {
                    EnsureThatAllOperationsHaveNumber(order);

                    var originalEntity = dataExistOrders.FirstOrDefault(x => x.Id == order.Id);                
                    //Calculate order totals
                    TotalsCalculator.CalculateTotals(order);                 
                   
                    var modifiedEntity = AbstractTypeFactory<CustomerOrderEntity>.TryCreateInstance()
                                                                                 .FromModel(order, pkMap) as CustomerOrderEntity;
                    if (originalEntity != null)
                    {
                        changeTracker.Attach(originalEntity);
                        changedEntries.Add(new GenericChangedEntry<CustomerOrder>(order, (CustomerOrder)originalEntity.ToModel(AbstractTypeFactory<CustomerOrder>.TryCreateInstance()), EntryState.Modified));
                        modifiedEntity?.Patch(originalEntity);                       
                    }
                    else
                    {
                        repository.Add(modifiedEntity);
                        changedEntries.Add(new GenericChangedEntry<CustomerOrder>(order, EntryState.Added));
                    }
                }

                //Raise domain events
                EventPublisher.Publish(new OrderChangeEvent(changedEntries));
                CommitChanges(repository);
                pkMap.ResolvePrimaryKeys();
            }

            //Save dynamic properties
            foreach (var order in orders)
            {
                DynamicPropertyService.SaveDynamicPropertyValues(order);
            }
            //Raise domain events
            EventPublisher.Publish(new OrderChangedEvent(changedEntries));
        }

        public virtual CustomerOrder[] GetByIds(string[] orderIds, string responseGroup = null)
        {
            var retVal = new List<CustomerOrder>();
            var orderResponseGroup = EnumUtility.SafeParse(responseGroup, CustomerOrderResponseGroup.Full);

            using (var repository = RepositoryFactory())
            {
                var orderEntities = repository.GetCustomerOrdersByIds(orderIds, orderResponseGroup);
                foreach (var orderEntity in orderEntities)
                {
                    var customerOrder = AbstractTypeFactory<CustomerOrder>.TryCreateInstance();
                    if (customerOrder != null)
                    {
                        customerOrder = orderEntity.ToModel(customerOrder) as CustomerOrder;
                        if (customerOrder != null)
                        {
                            var shippingMethods = ShippingMethodsService.GetAllShippingMethods();
                            if (!shippingMethods.IsNullOrEmpty())
                            {
                                foreach (var shipment in customerOrder.Shipments)
                                {
                                    shipment.ShippingMethod = shippingMethods.FirstOrDefault(x => x.Code.EqualsInvariant(shipment.ShipmentMethodCode));
                                }
                            }

                            var paymentMethods = PaymentMethodsService.GetAllPaymentMethods();
                            if (!paymentMethods.IsNullOrEmpty())
                            {
                                foreach (var payment in customerOrder.InPayments)
                                {
                                    payment.PaymentMethod = paymentMethods.FirstOrDefault(x => x.Code.EqualsInvariant(payment.GatewayCode));
                                }
                            }
                        }
                        //Calculate totals only for full responseGroup
                        if (orderResponseGroup == CustomerOrderResponseGroup.Full)
                        {
                            TotalsCalculator.CalculateTotals(customerOrder);
                        }
                        retVal.Add(customerOrder);
                    }
                }
            }

            DynamicPropertyService.LoadDynamicPropertyValues(retVal.ToArray<IHasDynamicProperties>());

            foreach (var order in retVal)
            {
                ChangeLogService.LoadChangeLogs(order);
                //Make general change log for order
                order.OperationsLog = order.GetFlatObjectsListWithInterface<IHasChangesHistory>()
                                           .Distinct()
                                           .SelectMany(x => x.OperationsLog)
                                           .OrderBy(x => x.CreatedDate)
                                           .Distinct().ToList();
            }

            return retVal.ToArray();
        }

        public virtual void Delete(string[] ids)
        {
            var orders = GetByIds(ids, CustomerOrderResponseGroup.Full.ToString());
            using (var repository = RepositoryFactory())
            {
                //Raise domain events before deletion
                var changedEntries = orders.Select(x => new GenericChangedEntry<CustomerOrder>(x, EntryState.Deleted));
                EventPublisher.Publish(new OrderChangeEvent(changedEntries));

                repository.RemoveOrdersByIds(ids);

                foreach (var order in orders)
                {
                    DynamicPropertyService.DeleteDynamicPropertyValues(order);
                }

                repository.UnitOfWork.Commit();
                //Raise domain events after deletion
                EventPublisher.Publish(new OrderChangedEvent(changedEntries));
            }
        }

        public virtual GenericSearchResult<CustomerOrder> SearchCustomerOrders(CustomerOrderSearchCriteria criteria)
        {
            using (var repository = RepositoryFactory())
            {
                var query = GetOrdersQuery(repository, criteria);
                var totalCount = query.Count();

                var sortInfos = criteria.SortInfos;
                if (sortInfos.IsNullOrEmpty())
                {
                    sortInfos = new[] { new SortInfo { SortColumn = ReflectionUtility.GetPropertyName<CustomerOrderEntity>(x => x.CreatedDate), SortDirection = SortDirection.Descending } };
                }
                query = query.OrderBySortInfos(sortInfos);

                var orderIds = query.Select(x => x.Id).Skip(criteria.Skip).Take(criteria.Take).ToArray();
                var orders = GetByIds(orderIds, criteria.ResponseGroup);

                var retVal = new GenericSearchResult<CustomerOrder>
                {
                    TotalCount = totalCount,
                    Results = orders.AsQueryable().OrderBySortInfos(sortInfos).ToList()
                };

                return retVal;
            }
        }

        #endregion


        protected virtual IQueryable<CustomerOrderEntity> GetOrdersQuery(IOrderRepository repository, CustomerOrderSearchCriteria criteria)
        {
            var query = repository.CustomerOrders;

            // Don't return prototypes by default
            if (!criteria.WithPrototypes)
            {
                query = query.Where(x => !x.IsPrototype);
            }

            if (criteria.OnlyRecurring)
            {
                query = query.Where(x => x.SubscriptionId != null);
            }

            if (criteria.CustomerId != null)
            {
                query = query.Where(x => x.CustomerId == criteria.CustomerId);
            }

            if (criteria.EmployeeId != null)
            {
                query = query.Where(x => x.EmployeeId == criteria.EmployeeId);
            }

            if (criteria.StartDate != null)
            {
                query = query.Where(x => x.CreatedDate >= criteria.StartDate);
            }

            if (criteria.EndDate != null)
            {
                query = query.Where(x => x.CreatedDate <= criteria.EndDate);
            }

            if (!criteria.SubscriptionIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.SubscriptionIds.Contains(x.SubscriptionId));
            }

            if (criteria.Statuses != null && criteria.Statuses.Any())
            {
                query = query.Where(x => criteria.Statuses.Contains(x.Status));
            }

            if (criteria.StoreIds != null && criteria.StoreIds.Any())
            {
                query = query.Where(x => criteria.StoreIds.Contains(x.StoreId));
            }

            if (!criteria.Numbers.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.Numbers.Contains(x.Number));
            }
            else if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(GetKeywordPredicate(criteria));
            }

            return query;
        }

        protected virtual Expression<Func<CustomerOrderEntity, bool>> GetKeywordPredicate(CustomerOrderSearchCriteria criteria)
        {
            return order => order.Number.Contains(criteria.Keyword) || order.CustomerName.Contains(criteria.Keyword);
        }

        protected virtual void EnsureThatAllOperationsHaveNumber(CustomerOrder order)
        {
            var store = StoreService.GetById(order.StoreId);

            foreach (var operation in order.GetFlatObjectsListWithInterface<Domain.Commerce.Model.IOperation>())
            {
                if (operation.Number == null)
                {
                    var objectTypeName = operation.OperationType;

                    // take uppercase chars to form operation type, or just take 2 first chars. (CustomerOrder => CO, PaymentIn => PI, Shipment => SH)
                    var opType = string.Concat(objectTypeName.Select(c => char.IsUpper(c) ? c.ToString() : ""));
                    if (opType.Length < 2)
                    {
                        opType = objectTypeName.Substring(0, 2).ToUpper();
                    }

                    var numberTemplate = opType + "{0:yyMMdd}-{1:D5}";
                    if (store != null)
                    {
                        numberTemplate = store.Settings.GetSettingValue("Order." + objectTypeName + "NewNumberTemplate", numberTemplate);
                    }

                    operation.Number = UniqueNumberGenerator.GenerateNumber(numberTemplate);
                }
            }
        }
    }
}
