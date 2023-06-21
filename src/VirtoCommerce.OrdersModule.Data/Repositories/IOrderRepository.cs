using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Data.Repositories
{
    public interface IOrderRepository : IRepository
    {
        IQueryable<CustomerOrderEntity> CustomerOrders { get; }
        IQueryable<ShipmentEntity> Shipments { get; }
        IQueryable<PaymentInEntity> InPayments { get; }
        IQueryable<AddressEntity> Addresses { get; }
        IQueryable<LineItemEntity> LineItems { get; }

        Task<CustomerOrderEntity[]> GetCustomerOrdersByIdsAsync(string[] ids, string responseGroup = null);
        Task<PaymentInEntity[]> GetPaymentsByIdsAsync(string[] ids);
        Task<ShipmentEntity[]> GetShipmentsByIdsAsync(string[] ids);
        Task RemoveOrdersByIdsAsync(string[] ids);

        /// <summary>
        /// Patch RowVersion to throw DBConcurrencyException exception if someone updated order before.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="rowVersion"></param>
        void PatchRowVersion(CustomerOrderEntity entity, byte[] rowVersion);
    }
}
