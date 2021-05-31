using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.OrdersModule.Data.Repositories
{
    public class OrderRepository : DbContextRepositoryBase<OrderDbContext>, IOrderRepository
    {
        public OrderRepository(OrderDbContext dbContext, IUnitOfWork unitOfWork = null) : base(dbContext, unitOfWork)
        {
        }

        public IQueryable<CustomerOrderEntity> CustomerOrders => DbContext.Set<CustomerOrderEntity>();
        public IQueryable<ShipmentEntity> Shipments => DbContext.Set<ShipmentEntity>();
        public IQueryable<ShipmentPackageEntity> ShipmentPackagesPackages => DbContext.Set<ShipmentPackageEntity>();
        public IQueryable<ShipmentItemEntity> ShipmentItems => DbContext.Set<ShipmentItemEntity>();
        public IQueryable<DiscountEntity> Discounts => DbContext.Set<DiscountEntity>();
        public IQueryable<TaxDetailEntity> TaxDetails => DbContext.Set<TaxDetailEntity>();
        public IQueryable<PaymentInEntity> InPayments => DbContext.Set<PaymentInEntity>();
        public IQueryable<AddressEntity> Addresses => DbContext.Set<AddressEntity>();
        public IQueryable<LineItemEntity> LineItems => DbContext.Set<LineItemEntity>();
        public IQueryable<PaymentGatewayTransactionEntity> Transactions => DbContext.Set<PaymentGatewayTransactionEntity>();
        public IQueryable<OrderDynamicPropertyObjectValueEntity> OrderDynamicPropertyObjectValues => DbContext.Set<OrderDynamicPropertyObjectValueEntity>();

        public virtual async Task<CustomerOrderEntity[]> GetCustomerOrdersByIdsAsync(string[] ids, string responseGroup = null)
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
                var lineItems = await LineItems.Where(x => ids.Contains(x.CustomerOrderId)).ToArrayAsync();

                if (lineItems.Any())
                {
                    var lineItemIds = lineItems.Select(x => x.Id).ToArray();

                    await Discounts.Where(x => lineItemIds.Contains(x.LineItemId)).LoadAsync();
                    await TaxDetails.Where(x => lineItemIds.Contains(x.LineItemId)).LoadAsync();

                    if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithDynamicProperties))
                    {
                        await OrderDynamicPropertyObjectValues.Where(x => lineItemIds.Contains(x.LineItemId)).LoadAsync();
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
                    await Addresses.Where(x => shipmentIds.Contains(x.ShipmentId)).LoadAsync();
                    await ShipmentItems.Where(x => shipmentIds.Contains(x.ShipmentId)).LoadAsync();
                    await ShipmentPackagesPackages.Where(x => shipmentIds.Contains(x.ShipmentId)).LoadAsync();

                    if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithDynamicProperties))
                    {
                        await OrderDynamicPropertyObjectValues.Where(x => shipmentIds.Contains(x.ShipmentId)).LoadAsync();
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

        public virtual async Task<PaymentInEntity[]> GetPaymentsByIdsAsync(string[] ids, string responseGroup = null)
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
            await Addresses.Where(x => ids.Contains(x.PaymentInId)).LoadAsync();
            await Transactions.Where(x => ids.Contains(x.PaymentInId)).LoadAsync();
            await OrderDynamicPropertyObjectValues.Where(x => ids.Contains(x.PaymentInId)).LoadAsync();

            return result;
        }

        public virtual async Task RemoveOrdersByIdsAsync(string[] ids)
        {
            var orders = await GetCustomerOrdersByIdsAsync(ids);
            foreach (var order in orders)
            {
                Remove(order);
            }
        }
    }
}
