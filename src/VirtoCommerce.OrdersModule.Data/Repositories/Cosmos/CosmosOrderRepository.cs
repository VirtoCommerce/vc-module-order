using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.OrdersModule.Data.Repositories.Cosmos
{
    public class CosmosOrderRepository : IOrderRepository
    {
        public CosmosOrderDBContext DbContext { get; }

        public CosmosOrderRepository(CosmosOrderDBContext dbContext, IUnitOfWork unitOfWork = null)
        {
            DbContext = dbContext;

            UnitOfWork = unitOfWork ?? new DbContextUnitOfWork(dbContext);
        }

        public IQueryable<CustomerOrderEntity> CustomerOrders => DbContext.Set<CustomerOrderEntity>();

        //public IQueryable<ShipmentEntity> Shipments => DbContext.Set<ShipmentEntity>();

        //public IQueryable<PaymentInEntity> InPayments => DbContext.Set<PaymentInEntity>();

        //public IQueryable<AddressEntity> Addresses => DbContext.Set<AddressEntity>();

        //public IQueryable<LineItemEntity> LineItems => DbContext.Set<LineItemEntity>();

        public IUnitOfWork UnitOfWork { get; private set; }

        public async Task<CustomerOrderEntity[]> GetCustomerOrdersByIdsAsync(string[] ids, string responseGroup = null)
        {
            if (ids.IsNullOrEmpty())
            {
                return Array.Empty<CustomerOrderEntity>();
            }

            var result = await CustomerOrders.Where(x => ids.Contains(x.Id)).ToArrayAsync();

            //foreach (var order in result)
            //{
            //    order.Shipments = new System.Collections.ObjectModel.ObservableCollection<ShipmentEntity>(await Shipments.Where(x => x.CustomerOrderId == order.Id).ToArrayAsync());
            //    order.InPayments = new System.Collections.ObjectModel.ObservableCollection<PaymentInEntity>(await InPayments.Where(x => x.CustomerOrderId == order.Id).ToArrayAsync());
            //    order.Addresses = new System.Collections.ObjectModel.ObservableCollection<AddressEntity>(await Addresses.Where(x => x.CustomerOrderId == order.Id).ToArrayAsync());
            //    order.Items = new System.Collections.ObjectModel.ObservableCollection<LineItemEntity>(await LineItems.Where(x => x.CustomerOrderId == order.Id).ToArrayAsync());
            //}

            return result;
        }

        public async Task<PaymentInEntity[]> GetPaymentsByIdsAsync(string[] ids, string responseGroup = null)
        {
            //if (ids.IsNullOrEmpty())
            //{
            //    return Array.Empty<PaymentInEntity>();
            //}

            //var result = await InPayments.Where(x => ids.Contains(x.Id)).ToArrayAsync();
            //return result;
            return null;
        }

        public async Task RemoveOrdersByIdsAsync(string[] ids)
        {
            var orders = await GetCustomerOrdersByIdsAsync(ids);
            foreach (var order in orders)
            {
                Remove(order);
            }
        }

        public void Attach<T>(T item) where T : class
        {
            DbContext.Attach(item);
        }

        public void Add<T>(T item) where T : class
        {
            var allEntities = item.GetFlatObjectsListWithInterface<IEntity>();
            foreach (var entity in allEntities)
            {
                entity.Id = Guid.NewGuid().ToString(); // ValueGeneratedOnAdd does not working

                if (entity is AddressEntity address)
                {
                    address.CustomerOrderId = (item as IEntity).Id;
                }

                if (entity is ShipmentEntity shipment)
                {
                    shipment.CustomerOrderId = (item as IEntity).Id;
                }

                if (entity is PaymentInEntity payment)
                {
                    payment.CustomerOrderId = (item as IEntity).Id;
                }

                if (entity is LineItemEntity lineItem)
                {
                    lineItem.CustomerOrderId = (item as IEntity).Id;
                }

                if (entity is DiscountEntity discount)
                {
                    discount.CustomerOrderId = (item as IEntity).Id;
                }

                if (entity is OrderDynamicPropertyObjectValueEntity dynProp)
                {
                    dynProp.CustomerOrderId = (item as IEntity).Id;
                }

                if (entity is TaxDetailEntity taxDetail)
                {
                    taxDetail.CustomerOrderId = (item as IEntity).Id;
                }

            }

            DbContext.Add(item);
        }

        public void Update<T>(T item) where T : class
        {
            DbContext.Update(item);
            DbContext.Entry(item).State = EntityState.Modified;
        }

        public void Remove<T>(T item) where T : class
        {
            DbContext.Remove(item);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && DbContext != null)
            {
                DbContext.Dispose();
                UnitOfWork = null;
            }
        }
    }
}
