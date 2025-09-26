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
#pragma warning disable S109
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

            var result = await CustomerOrders.Where(x => ids.Contains(x.Id)).ToArrayAsync();

            if (!result.Any())
            {
                return Array.Empty<CustomerOrderEntity>();
            }

            ids = result.Select(x => x.Id).ToArray();

            await Discounts.Where(x => ids.Contains(x.CustomerOrderId)).LoadAsync();
            await TaxDetails.Where(x => ids.Contains(x.CustomerOrderId)).LoadAsync();
            await FeeDetails.Where(x => ids.Contains(x.CustomerOrderId)).LoadAsync();

            var customerOrderResponseGroup = EnumUtility.SafeParseFlags(responseGroup, CustomerOrderResponseGroup.Full);

            if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithDynamicProperties))
            {
                await OrderDynamicPropertyObjectValues.Where(x => ids.Contains(x.CustomerOrderId)).LoadAsync();
            }

            if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithAddresses))
            {
                await Addresses.Where(x => ids.Contains(x.CustomerOrderId)).LoadAsync();
            }

            if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithInPayments))
            {
                var payments = await InPayments.Where(x => ids.Contains(x.CustomerOrderId)).ToArrayAsync();

                if (payments.Any())
                {
                    var paymentIds = payments.Select(x => x.Id).ToArray();

                    await Discounts.Where(x => paymentIds.Contains(x.PaymentInId)).LoadAsync();
                    await TaxDetails.Where(x => paymentIds.Contains(x.PaymentInId)).LoadAsync();
                    await FeeDetails.Where(x => paymentIds.Contains(x.PaymentInId)).LoadAsync();
                    await Addresses.Where(x => paymentIds.Contains(x.PaymentInId)).LoadAsync();
                    await Transactions.Where(x => paymentIds.Contains(x.PaymentInId)).LoadAsync();

                    if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithDynamicProperties))
                    {
                        await OrderDynamicPropertyObjectValues.Where(x => paymentIds.Contains(x.PaymentInId)).LoadAsync();
                    }
                }
            }

            if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithItems))
            {
                var lineItems = await LineItems
                    .Where(x => ids.Contains(x.CustomerOrderId))
                    .ToArrayAsync();

                if (lineItems.Length > 0)
                {
                    var lineItemIds = lineItems.Select(x => x.Id).ToArray();

                    await Discounts.Where(x => lineItemIds.Contains(x.LineItemId)).LoadAsync();
                    await TaxDetails.Where(x => lineItemIds.Contains(x.LineItemId)).LoadAsync();
                    await FeeDetails.Where(x => lineItemIds.Contains(x.LineItemId)).LoadAsync();

                    if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithDynamicProperties))
                    {
                        await OrderDynamicPropertyObjectValues.Where(x => lineItemIds.Contains(x.LineItemId)).LoadAsync();
                    }

                    var configurationItemIds = lineItems.Where(x => x.IsConfigured).Select(x => x.Id).ToArray();
                    if (configurationItemIds.Length > 0)
                    {
                        await ConfigurationItems
                            .Where(x => configurationItemIds.Contains(x.LineItemId))
                            .Include(x => x.Files)
                            .AsSplitQuery()
                            .LoadAsync();
                    }
                }
            }

            if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithShipments))
            {
                var shipments = await Shipments.Where(x => ids.Contains(x.CustomerOrderId)).ToArrayAsync();

                if (shipments.Any())
                {
                    var shipmentIds = shipments.Select(x => x.Id).ToArray();

                    await Discounts.Where(x => shipmentIds.Contains(x.ShipmentId)).LoadAsync();
                    await TaxDetails.Where(x => shipmentIds.Contains(x.ShipmentId)).LoadAsync();
                    await FeeDetails.Where(x => shipmentIds.Contains(x.ShipmentId)).LoadAsync();
                    await Addresses.Where(x => shipmentIds.Contains(x.ShipmentId)).LoadAsync();
                    await ShipmentItems.Where(x => shipmentIds.Contains(x.ShipmentId)).LoadAsync();
                    await ShipmentPackagesPackages.Where(x => shipmentIds.Contains(x.ShipmentId)).LoadAsync();

                    if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithDynamicProperties))
                    {
                        await OrderDynamicPropertyObjectValues.Where(x => shipmentIds.Contains(x.ShipmentId)).LoadAsync();
                    }
                }
            }


            if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithRefunds))
            {
                var refunds = await Refunds.Where(x => ids.Contains(x.CustomerOrderId)).ToArrayAsync();

                if (refunds.Any())
                {
                    var refundIds = refunds.Select(x => x.Id).ToArray();

                    await RefundItems.Where(x => refundIds.Contains(x.RefundId)).LoadAsync();

                    if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithDynamicProperties))
                    {
                        await OrderDynamicPropertyObjectValues.Where(x => refundIds.Contains(x.RefundId)).LoadAsync();
                    }
                }
            }

            if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithCaptures))
            {
                var captures = await Captures.Where(x => ids.Contains(x.CustomerOrderId)).ToArrayAsync();

                if (captures.Any())
                {
                    var captureIds = captures.Select(x => x.Id).ToArray();

                    await CaptureItems.Where(x => captureIds.Contains(x.CaptureId)).LoadAsync();

                    if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithDynamicProperties))
                    {
                        await OrderDynamicPropertyObjectValues.Where(x => captureIds.Contains(x.CaptureId)).LoadAsync();
                    }
                }
            }

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

            var result = await InPayments.Where(x => ids.Contains(x.Id)).ToArrayAsync();

            if (!result.Any())
            {
                return Array.Empty<PaymentInEntity>();
            }

            ids = result.Select(x => x.Id).ToArray();

            await Discounts.Where(x => ids.Contains(x.PaymentInId)).LoadAsync();
            await TaxDetails.Where(x => ids.Contains(x.PaymentInId)).LoadAsync();
            await FeeDetails.Where(x => ids.Contains(x.PaymentInId)).LoadAsync();
            await Addresses.Where(x => ids.Contains(x.PaymentInId)).LoadAsync();
            await Transactions.Where(x => ids.Contains(x.PaymentInId)).LoadAsync();
            await OrderDynamicPropertyObjectValues.Where(x => ids.Contains(x.PaymentInId)).LoadAsync();

            return result;
        }

        public virtual async Task<IList<ShipmentEntity>> GetShipmentsByIdsAsync(IList<string> ids)
        {
            if (ids.IsNullOrEmpty())
            {
                return Array.Empty<ShipmentEntity>();
            }

            var result = await Shipments.Where(x => ids.Contains(x.Id)).ToArrayAsync();

            if (!result.Any())
            {
                return Array.Empty<ShipmentEntity>();
            }

            ids = result.Select(x => x.Id).ToArray();

            await Discounts.Where(x => ids.Contains(x.ShipmentId)).LoadAsync();
            await TaxDetails.Where(x => ids.Contains(x.ShipmentId)).LoadAsync();
            await FeeDetails.Where(x => ids.Contains(x.ShipmentId)).LoadAsync();
            await Addresses.Where(x => ids.Contains(x.ShipmentId)).LoadAsync();
            await ShipmentItems.Where(x => ids.Contains(x.ShipmentId)).LoadAsync();
            await ShipmentPackagesPackages.Where(x => ids.Contains(x.ShipmentId)).LoadAsync();
            await OrderDynamicPropertyObjectValues.Where(x => ids.Contains(x.ShipmentId)).LoadAsync();

            return result;
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
#pragma warning restore S109

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
