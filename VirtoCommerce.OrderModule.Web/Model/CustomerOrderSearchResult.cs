using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Order.Model;

namespace VirtoCommerce.OrderModule.Web.Model
{
	public class CustomerOrderSearchResult
	{
		public CustomerOrderSearchResult()
		{
			CustomerOrders = new List<CustomerOrder>();
		}
		public int TotalCount { get; set; }

		public List<CustomerOrder> CustomerOrders { get; set; }

	}
}
