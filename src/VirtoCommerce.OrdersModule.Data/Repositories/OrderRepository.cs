using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.OrdersModule.Data.Repositories
{
    public class OrderRepository : DbContextRepositoryBase<OrderDbContext>, IOrderRepository
    {
        public OrderRepository(OrderDbContext dbContext, IUnitOfWork unitOfWork = null)
            : base(dbContext, unitOfWork)
        {
            // Resolves Breaking changes in EF Core 7.0 (EF7) when EF Core will not automatically delete orphans because all FKs are nullable.
            // https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/breaking-changes?tabs=v7#orphaned-dependents-of-optional-relationships-are-not-automatically-deleted
            dbContext.SavingChanges += OnSavingChanges;
        }

        public IQueryable<CustomerOrderEntity> CustomerOrders => DbContext.Set<CustomerOrderEntity>();
        public IQueryable<ShipmentEntity> Shipments => DbContext.Set<ShipmentEntity>();
        public IQueryable<ShipmentPackageEntity> ShipmentPackagesPackages => DbContext.Set<ShipmentPackageEntity>();
        public IQueryable<ShipmentItemEntity> ShipmentItems => DbContext.Set<ShipmentItemEntity>();
        public IQueryable<DiscountEntity> Discounts => DbContext.Set<DiscountEntity>();
        public IQueryable<TaxDetailEntity> TaxDetails => DbContext.Set<TaxDetailEntity>();
        public IQueryable<FeeDetailEntity> FeeDetails => DbContext.Set<FeeDetailEntity>();
        public IQueryable<PaymentInEntity> InPayments => DbContext.Set<PaymentInEntity>();
        public IQueryable<AddressEntity> Addresses => DbContext.Set<AddressEntity>();
        public IQueryable<LineItemEntity> LineItems => DbContext.Set<LineItemEntity>();
        public IQueryable<PaymentGatewayTransactionEntity> Transactions => DbContext.Set<PaymentGatewayTransactionEntity>();
        public IQueryable<RefundEntity> Refunds => DbContext.Set<RefundEntity>();
        public IQueryable<RefundItemEntity> RefundItems => DbContext.Set<RefundItemEntity>();
        public IQueryable<CaptureEntity> Captures => DbContext.Set<CaptureEntity>();
        public IQueryable<CaptureItemEntity> CaptureItems => DbContext.Set<CaptureItemEntity>();
        public IQueryable<ConfigurationItemEntity> ConfigurationItems => DbContext.Set<ConfigurationItemEntity>();
        public IQueryable<ConfigurationItemFileEntity> ConfigurationItemFiles => DbContext.Set<ConfigurationItemFileEntity>();

        public IQueryable<OrderDynamicPropertyObjectValueEntity> OrderDynamicPropertyObjectValues => DbContext.Set<OrderDynamicPropertyObjectValueEntity>();

        public virtual async Task<IList<CustomerOrderEntity>> GetCustomerOrdersByIdsAsync(IList<string> ids, string responseGroup = null)
        {
            if (ids.IsNullOrEmpty())
            {
                return Array.Empty<CustomerOrderEntity>();
            }

            var customerOrderResponseGroup = EnumUtility.SafeParseFlags(responseGroup, CustomerOrderResponseGroup.Full);

            // Load CustomerOrder + the child collections that hang directly off it in a single
            // round-trip. EF Core wires up navigations on InPayments/Items/Shipments that are
            // loaded below by foreign-key fix-up.
            var rootQuery = CustomerOrders
                .Include(x => x.Discounts)
                .Include(x => x.TaxDetails)
                .Include(x => x.FeeDetails)
                .AsQueryable();

            if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithAddresses))
            {
                rootQuery = rootQuery.Include(x => x.Addresses);
            }

            if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithDynamicProperties))
            {
                rootQuery = rootQuery.Include(x => x.DynamicPropertyObjectValues);
            }

            var result = await rootQuery
                .Where(x => ids.Contains(x.Id))
                .AsSplitQuery()
                .ToArrayAsync();

            if (result.Length == 0)
            {
                return Array.Empty<CustomerOrderEntity>();
            }

            ids = result.Select(x => x.Id).ToArray();

            await LoadInPayments(ids, customerOrderResponseGroup);
            await LoadLineItems(ids, customerOrderResponseGroup);
            await LoadShipments(ids, customerOrderResponseGroup);
            await LoadRefunds(ids, customerOrderResponseGroup);
            await LoadCaptures(ids, customerOrderResponseGroup);

            if (!customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithPrices))
            {
                foreach (var customerOrder in result)
                {
                    customerOrder.ResetPrices();
                }
            }

            return result;
        }

        public virtual async Task<IList<PaymentInEntity>> GetPaymentsByIdsAsync(IList<string> ids)
        {
            if (ids.IsNullOrEmpty())
            {
                return Array.Empty<PaymentInEntity>();
            }

            var result = await InPayments
                .Include(x => x.Discounts)
                .Include(x => x.TaxDetails)
                .Include(x => x.FeeDetails)
                .Include(x => x.Addresses)
                .Include(x => x.Transactions)
                .Include(x => x.DynamicPropertyObjectValues)
                .Where(x => ids.Contains(x.Id))
                .AsSplitQuery()
                .ToArrayAsync();

            return result.Length != 0 ? result : Array.Empty<PaymentInEntity>();
        }

        public virtual async Task<IList<ShipmentEntity>> GetShipmentsByIdsAsync(IList<string> ids)
        {
            if (ids.IsNullOrEmpty())
            {
                return Array.Empty<ShipmentEntity>();
            }

            var result = await Shipments
                .Include(x => x.Discounts)
                .Include(x => x.TaxDetails)
                .Include(x => x.FeeDetails)
                .Include(x => x.Addresses)
                .Include(x => x.Items)
                .Include(x => x.Packages)
                .Include(x => x.DynamicPropertyObjectValues)
                .Where(x => ids.Contains(x.Id))
                .AsSplitQuery()
                .ToArrayAsync();

            return result.Length != 0 ? result : Array.Empty<ShipmentEntity>();
        }

        protected virtual async Task LoadInPayments(IList<string> orderIds, CustomerOrderResponseGroup responseGroup)
        {
            if (!responseGroup.HasFlag(CustomerOrderResponseGroup.WithInPayments))
            {
                return;
            }

            var query = InPayments
                .Include(x => x.Discounts)
                .Include(x => x.TaxDetails)
                .Include(x => x.FeeDetails)
                .Include(x => x.Addresses)
                .Include(x => x.Transactions)
                .AsQueryable();

            if (responseGroup.HasFlag(CustomerOrderResponseGroup.WithDynamicProperties))
            {
                query = query.Include(x => x.DynamicPropertyObjectValues);
            }

            await query
                .Where(x => orderIds.Contains(x.CustomerOrderId))
                .AsSplitQuery()
                .LoadAsync();
        }

        protected virtual async Task LoadLineItems(IList<string> orderIds, CustomerOrderResponseGroup responseGroup)
        {
            if (!responseGroup.HasFlag(CustomerOrderResponseGroup.WithItems))
            {
                return;
            }

            var query = LineItems
                .Include(x => x.Discounts)
                .Include(x => x.TaxDetails)
                .Include(x => x.FeeDetails)
                .AsQueryable();

            if (responseGroup.HasFlag(CustomerOrderResponseGroup.WithDynamicProperties))
            {
                query = query.Include(x => x.DynamicPropertyObjectValues);
            }

            var lineItems = await query
                .Where(x => orderIds.Contains(x.CustomerOrderId))
                .AsSplitQuery()
                .ToArrayAsync();

            // ConfigurationItem.Files is two levels deeper than LineItem; keeping it as a
            // dedicated query avoids growing the LineItem join tree (and the cartesian
            // explosion under AsSingleQuery semantics).
            var configurationItemIds = lineItems.Where(x => x.IsConfigured).Select(x => x.Id).ToArray();
            if (configurationItemIds.Length > 0)
            {
                await ConfigurationItems
                    .Include(x => x.Files)
                    .Where(x => configurationItemIds.Contains(x.LineItemId))
                    .AsSplitQuery()
                    .LoadAsync();
            }
        }

        protected virtual async Task LoadShipments(IList<string> orderIds, CustomerOrderResponseGroup responseGroup)
        {
            if (!responseGroup.HasFlag(CustomerOrderResponseGroup.WithShipments))
            {
                return;
            }

            var query = Shipments
                .Include(x => x.Discounts)
                .Include(x => x.TaxDetails)
                .Include(x => x.FeeDetails)
                .Include(x => x.Addresses)
                .Include(x => x.Items)
                .Include(x => x.Packages)
                .AsQueryable();

            if (responseGroup.HasFlag(CustomerOrderResponseGroup.WithDynamicProperties))
            {
                query = query.Include(x => x.DynamicPropertyObjectValues);
            }

            await query
                .Where(x => orderIds.Contains(x.CustomerOrderId))
                .AsSplitQuery()
                .LoadAsync();
        }

        protected virtual async Task LoadRefunds(IList<string> orderIds, CustomerOrderResponseGroup responseGroup)
        {
            if (!responseGroup.HasFlag(CustomerOrderResponseGroup.WithRefunds))
            {
                return;
            }

            var query = Refunds
                .Include(x => x.Items)
                .AsQueryable();

            if (responseGroup.HasFlag(CustomerOrderResponseGroup.WithDynamicProperties))
            {
                query = query.Include(x => x.DynamicPropertyObjectValues);
            }

            await query
                .Where(x => orderIds.Contains(x.CustomerOrderId))
                .AsSplitQuery()
                .LoadAsync();
        }

        protected virtual async Task LoadCaptures(IList<string> orderIds, CustomerOrderResponseGroup responseGroup)
        {
            if (!responseGroup.HasFlag(CustomerOrderResponseGroup.WithCaptures))
            {
                return;
            }

            var query = Captures
                .Include(x => x.Items)
                .AsQueryable();

            if (responseGroup.HasFlag(CustomerOrderResponseGroup.WithDynamicProperties))
            {
                query = query.Include(x => x.DynamicPropertyObjectValues);
            }

            await query
                .Where(x => orderIds.Contains(x.CustomerOrderId))
                .AsSplitQuery()
                .LoadAsync();
        }

        public void PatchRowVersion(CustomerOrderEntity entity, byte[] rowVersion)
        {
            if (rowVersion != null)
            {
                DbContext.Entry(entity).Property(e => e.RowVersion).OriginalValue = rowVersion;
            }
        }

        public virtual async Task RemoveOrdersByIdsAsync(IList<string> ids)
        {
            var orders = await GetCustomerOrdersByIdsAsync(ids);
            foreach (var order in orders)
            {
                Remove(order);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && DbContext != null)
            {
                DbContext.SavingChanges -= OnSavingChanges;
            }
            base.Dispose(disposing);
        }

        protected virtual bool IsOrphanedEntity(EntityEntry entry)
        {
            switch (entry.Entity)
            {
                case CaptureEntity capture
                    when capture.PaymentId == null:
                case RefundEntity refund
                    when refund.PaymentId == null:
                case PaymentInEntity payment
                    when payment.CustomerOrderId == null
                        && payment.ShipmentId == null:
                case AddressEntity a
                    when a.CustomerOrderId == null
                        && a.ShipmentId == null
                        && a.PaymentInId == null:
                case DiscountEntity d
                    when d.CustomerOrderId == null
                        && d.ShipmentId == null
                        && d.LineItemId == null
                        && d.PaymentInId == null:
                case TaxDetailEntity t
                    when t.CustomerOrderId == null
                        && t.ShipmentId == null
                        && t.LineItemId == null
                        && t.PaymentInId == null:
                case FeeDetailEntity f
                    when f.CustomerOrderId == null
                        && f.ShipmentId == null
                        && f.LineItemId == null
                        && f.PaymentInId == null:
                case OrderDynamicPropertyObjectValueEntity v
                    when v.CustomerOrderId == null
                      && v.PaymentInId == null
                      && v.ShipmentId == null
                      && v.RefundId == null
                      && v.LineItemId == null
                      && v.CaptureId == null:
                    return true;
            }

            return false;
        }

        private void OnSavingChanges(object sender, SavingChangesEventArgs args)
        {
            var ctx = (DbContext)sender;
            var entries = ctx.ChangeTracker.Entries();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Modified &&
                    IsOrphanedEntity(entry))
                {
                    entry.State = EntityState.Deleted;
                }
            }
        }
    }
}
