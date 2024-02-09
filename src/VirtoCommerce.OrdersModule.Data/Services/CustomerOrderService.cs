using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.PaymentModule.Core.Model.Search;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.ShippingModule.Core.Model.Search;
using VirtoCommerce.ShippingModule.Core.Services;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.OrdersModule.Data.Services
{
    public class CustomerOrderService : CrudService<CustomerOrder, CustomerOrderEntity, OrderChangeEvent, OrderChangedEvent>, ICustomerOrderService, IMemberOrdersService
    {
        private readonly Func<IOrderRepository> _repositoryFactory;
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly IEventPublisher _eventPublisher;
        private readonly ITenantUniqueNumberGenerator _uniqueNumberGenerator;
        private readonly IStoreService _storeService;
        private readonly ICustomerOrderTotalsCalculator _totalsCalculator;
        private readonly IShippingMethodsSearchService _shippingMethodsSearchService;
        private readonly IPaymentMethodsSearchService _paymentMethodSearchService;

        public CustomerOrderService(
            Func<IOrderRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher,
            ITenantUniqueNumberGenerator uniqueNumberGenerator,
            IStoreService storeService,
            ICustomerOrderTotalsCalculator totalsCalculator,
            IShippingMethodsSearchService shippingMethodsSearchService,
            IPaymentMethodsSearchService paymentMethodSearchService)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _repositoryFactory = repositoryFactory;
            _platformMemoryCache = platformMemoryCache;
            _eventPublisher = eventPublisher;
            _uniqueNumberGenerator = uniqueNumberGenerator;
            _storeService = storeService;
            _totalsCalculator = totalsCalculator;
            _shippingMethodsSearchService = shippingMethodsSearchService;
            _paymentMethodSearchService = paymentMethodSearchService;
        }

        public override async Task SaveChangesAsync(IList<CustomerOrder> models)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            var changedEntries = new List<GenericChangedEntry<CustomerOrder>>();
            var changedEntities = new List<CustomerOrderEntity>();

            await BeforeSaveChanges(models);

            using (var repository = _repositoryFactory())
            {
                var orderIds = models.Where(x => !x.IsTransient()).Select(x => x.Id).ToArray();
                var existingEntities = await repository.GetCustomerOrdersByIdsAsync(orderIds, CustomerOrderResponseGroup.Full.ToString());
                foreach (var modifiedOrder in models)
                {
                    await EnsureThatAllOperationsHaveNumber(modifiedOrder);
                    await LoadOrderDependenciesAsync(modifiedOrder);

                    var originalEntity = existingEntities?.FirstOrDefault(x => x.Id == modifiedOrder.Id);

                    if (originalEntity != null)
                    {
                        // Patch RowVersion to throw concurrency exception if someone updated order before
                        // https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations#optimistic-concurrency
                        // https://stackoverflow.com/questions/75454812/entity-framework-core-manually-changing-rowversion-of-entity-has-no-effect-on-c
                        repository.PatchRowVersion(originalEntity, modifiedOrder.RowVersion);

                        var oldModel = originalEntity.ToModel(AbstractTypeFactory<CustomerOrder>.TryCreateInstance());
                        await LoadOrderDependenciesAsync(oldModel);
                        _totalsCalculator.CalculateTotals(oldModel);

                        // Workaround to trigger update of auditable fields when only updating navigation properties.
                        // Otherwise on update trigger is fired only when non navigation properties are updated.
                        originalEntity.ModifiedDate = DateTime.UtcNow;

                        var modifiedEntity = AbstractTypeFactory<CustomerOrderEntity>.TryCreateInstance().FromModel(modifiedOrder, pkMap);
                        // This extension is allow to get around breaking changes is introduced in EF Core 3.0 that leads to throw
                        // Database operation expected to affect 1 row(s) but actually affected 0 row(s) exception when trying to add the new children entities with manually set keys
                        // https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-3.0/breaking-changes#detectchanges-honors-store-generated-key-values
                        repository.TrackModifiedAsAddedForNewChildEntities(originalEntity);

                        changedEntries.Add(new GenericChangedEntry<CustomerOrder>(modifiedOrder, oldModel, EntryState.Modified));
                        modifiedEntity?.Patch(originalEntity);

                        //originalEntity is fully loaded and contains changes from order
                        var newModel = originalEntity.ToModel(AbstractTypeFactory<CustomerOrder>.TryCreateInstance());

                        //newModel is fully loaded,so we can CalculateTotals for order
                        _totalsCalculator.CalculateTotals(newModel);
                        //Double convert and patch are required, because of partial order update when some properties are used in totals calculation are missed
                        var newModifiedEntity = AbstractTypeFactory<CustomerOrderEntity>.TryCreateInstance().FromModel(newModel, pkMap);
                        newModifiedEntity?.Patch(originalEntity);
                        changedEntities.Add(originalEntity);
                    }
                    else
                    {
                        _totalsCalculator.CalculateTotals(modifiedOrder);
                        var modifiedEntity = AbstractTypeFactory<CustomerOrderEntity>.TryCreateInstance().FromModel(modifiedOrder, pkMap);
                        repository.Add(modifiedEntity);
                        changedEntries.Add(new GenericChangedEntry<CustomerOrder>(modifiedOrder, EntryState.Added));
                        changedEntities.Add(modifiedEntity);
                    }
                }

                //Raise domain events
                await _eventPublisher.Publish(new OrderChangeEvent(changedEntries));

                try
                {
                    await repository.UnitOfWork.CommitAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw new DbUpdateConcurrencyException("The order has been modified by another user. Please reload the latest data and try again.");
                }
                finally
                {
                    ClearCache(models);
                }

                pkMap.ResolvePrimaryKeys();
            }

            // VP-5561: Need to fill changedEntries newEntry with the models built from saved entities (with the info filled when saving to database)
            foreach (var (changedEntry, i) in changedEntries.Select((x, i) => (x, i)))
            {
                var changedModel = changedEntities[i].ToModel(AbstractTypeFactory<CustomerOrder>.TryCreateInstance());

                // We need to CalculateTotals for the new Order, because it is empty after entity.ToModel creation
                _totalsCalculator.CalculateTotals(changedModel);

                await LoadOrderDependenciesAsync(changedModel);
                changedEntry.NewEntry = changedModel;
            }

            await AfterSaveChangesAsync(models, changedEntries);

            await _eventPublisher.Publish(new OrderChangedEvent(changedEntries));
        }

        public override async Task DeleteAsync(IList<string> ids, bool softDelete = false)
        {
            var orders = await GetAsync(ids, CustomerOrderResponseGroup.Full.ToString());
            using (var repository = _repositoryFactory())
            {
                //Raise domain events before deletion
                var changedEntries = orders.Select(x => new GenericChangedEntry<CustomerOrder>(x, EntryState.Deleted)).ToList();
                await _eventPublisher.Publish(new OrderChangeEvent(changedEntries));

                await repository.RemoveOrdersByIdsAsync(ids);

                await repository.UnitOfWork.CommitAsync();

                ClearCache(orders);
                //Raise domain events after deletion
                await _eventPublisher.Publish(new OrderChangedEvent(changedEntries));
            }
        }

        public virtual bool IsFirstTimeBuyer(string customerId)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(IsFirstTimeBuyer), customerId);
            var result = _platformMemoryCache.GetOrCreateExclusive(cacheKey, cacheEntry =>
            {
                cacheEntry.AddExpirationToken(CreateCacheToken(customerId));

                using var repository = _repositoryFactory();
                return !repository.CustomerOrders.Any(x => x.CustomerId == customerId);
            });

            return result;
        }

        protected virtual async Task LoadOrderDependenciesAsync(CustomerOrder order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            var searchShippingMethodsTask = _shippingMethodsSearchService.SearchAsync(new ShippingMethodsSearchCriteria { StoreId = order.StoreId });
            var searchPaymentMethodsTask = _paymentMethodSearchService.SearchAsync(new PaymentMethodsSearchCriteria { StoreId = order.StoreId });

            await Task.WhenAll(searchShippingMethodsTask, searchPaymentMethodsTask);
            if (!searchShippingMethodsTask.Result.Results.IsNullOrEmpty() && !order.Shipments.IsNullOrEmpty())
            {
                foreach (var shipment in order.Shipments)
                {
                    shipment.ShippingMethod = searchShippingMethodsTask.Result.Results.FirstOrDefault(x => x.Code.EqualsInvariant(shipment.ShipmentMethodCode));
                }
            }
            if (!searchPaymentMethodsTask.Result.Results.IsNullOrEmpty() && !order.InPayments.IsNullOrEmpty())
            {
                foreach (var payment in order.InPayments)
                {
                    payment.PaymentMethod = searchPaymentMethodsTask.Result.Results.FirstOrDefault(x => x.Code.EqualsInvariant(payment.GatewayCode));
                }
            }
        }

        protected virtual async Task EnsureThatAllOperationsHaveNumber(CustomerOrder order)
        {
            var store = await _storeService.GetNoCloneAsync(order.StoreId, StoreResponseGroup.StoreInfo.ToString());

            foreach (var operation in order.GetFlatObjectsListWithInterface<IOperation>())
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
                        var descriptor = new SettingDescriptor
                        {
                            Name = "Order." + objectTypeName + "NewNumberTemplate",
                            DefaultValue = numberTemplate,
                        };
                        numberTemplate = store.Settings.GetValue<string>(descriptor);
                    }

                    operation.Number = _uniqueNumberGenerator.GenerateNumber(order.StoreId, numberTemplate);
                }
            }
        }

        protected override Task<IList<CustomerOrderEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
        {
            return ((IOrderRepository)repository).GetCustomerOrdersByIdsAsync(ids, responseGroup);
        }

        protected override CustomerOrder ProcessModel(string responseGroup, CustomerOrderEntity entity, CustomerOrder model)
        {
            var orderResponseGroup = EnumUtility.SafeParseFlags(responseGroup, CustomerOrderResponseGroup.Full);

            //Calculate totals only for full responseGroup
            if (orderResponseGroup == CustomerOrderResponseGroup.Full)
            {
                _totalsCalculator.CalculateTotals(model);
            }

            LoadOrderDependenciesAsync(model).GetAwaiter().GetResult();
            model.ReduceDetails(responseGroup);
            return model;
        }

        protected override void ClearCache(IList<CustomerOrder> models)
        {
            GenericSearchCachingRegion<CustomerOrder>.ExpireRegion();

            foreach (var model in models)
            {
                GenericCachingRegion<CustomerOrder>.ExpireTokenForKey(model.Id);
                GenericCachingRegion<CustomerOrder>.ExpireTokenForKey(model.CustomerId);
            }
        }
    }
}
