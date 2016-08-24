using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.OrderModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrderModule.Data.Repositories
{
	public interface IOrderRepository : IRepository
	{
		IQueryable<CustomerOrderEntity> CustomerOrders { get; }
		IQueryable<ShipmentEntity> Shipments { get; }
		IQueryable<PaymentInEntity> InPayments { get; }
		IQueryable<AddressEntity> Addresses { get; }
		IQueryable<LineItemEntity> LineItems { get; }

		CustomerOrderEntity[] GetCustomerOrdersByIds(string[] ids, CustomerOrderResponseGroup responseGroup);
        void RemoveOrdersByIds(string[] ids);
	}
}
