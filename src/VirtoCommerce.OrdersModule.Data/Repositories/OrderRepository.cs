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
            if(ids.IsNullOrEmpty())
            {
                return Array.Empty<CustomerOrderEntity>();
            }

            var customerOrderResponseGroup = EnumUtility.SafeParseFlags(responseGroup, CustomerOrderResponseGroup.Full);
            var query = CustomerOrders.Where(x => ids.Contains(x.Id))
                                             .Include(x=> x.Discounts)
                                             .Include(x=> x.TaxDetails).AsQueryable();

      
            var result = await query.ToArrayAsync();

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
                IQueryable<PaymentInEntity> inPaymentsLoadBreakingQuery = InPayments.Where(x => ids.Contains(x.CustomerOrderId))
                                                 .Include(x => x.Discounts)
                                                 .Include(x => x.TaxDetails)
                                                 .Include(x => x.Addresses)
                                                 .Include(x => x.Transactions);

                if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithDynamicProperties))
                {
                    inPaymentsLoadBreakingQuery =  inPaymentsLoadBreakingQuery.Include(x => x.DynamicPropertyObjectValues);
                }
                await inPaymentsLoadBreakingQuery.LoadAsync();
            }

            if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithItems))
            {
                IQueryable<LineItemEntity> itemsLoadBreakingQuery = LineItems.Where(x => ids.Contains(x.CustomerOrderId))
                                                .Include(x => x.Discounts)
                                                .Include(x => x.TaxDetails)
                                                .Include(x => x.DynamicPropertyObjectValues);
                if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithDynamicProperties))
                {
                    itemsLoadBreakingQuery = itemsLoadBreakingQuery.Include(x => x.DynamicPropertyObjectValues);
                }
                await itemsLoadBreakingQuery.LoadAsync();
            }

            if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithShipments))
            {
                IQueryable<ShipmentEntity> shipmentLoadBreakingQuery = Shipments.Where(x => ids.Contains(x.CustomerOrderId))
                                               .Include(x => x.Discounts)
                                               .Include(x => x.TaxDetails)
                                               .Include(x => x.Addresses)
                                               .Include(x => x.Items)
                                               .Include(x => x.Packages);

                if (customerOrderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithDynamicProperties))
                {
                    shipmentLoadBreakingQuery = shipmentLoadBreakingQuery.Include(x => x.DynamicPropertyObjectValues);
                }
                await shipmentLoadBreakingQuery.LoadAsync();
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
