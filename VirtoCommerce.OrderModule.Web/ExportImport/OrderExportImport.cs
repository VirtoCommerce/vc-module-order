using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Domain.Order.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;

namespace VirtoCommerce.OrderModule.Web.ExportImport
{
    public sealed class BackupObject
    {
        public BackupObject()
        {
            CustomerOrders = new List<CustomerOrder>();
        }
        public ICollection<CustomerOrder> CustomerOrders { get; set; }
    }

    public sealed class OrderExportImport 
    {
        private readonly ICustomerOrderSearchService _customerOrderSearchService;
        private readonly ICustomerOrderService _customerOrderService;

        public OrderExportImport(ICustomerOrderSearchService customerOrderSearchService, ICustomerOrderService customerOrderService)
        {
            _customerOrderSearchService = customerOrderSearchService;
            _customerOrderService = customerOrderService;
        }

		public void DoExport(Stream backupStream, Action<ExportImportProgressInfo> progressCallback)
        {
			var backupObject = GetBackupObject(progressCallback); 
            backupObject.SerializeJson(backupStream);
        }

		public void DoImport(Stream backupStream, Action<ExportImportProgressInfo> progressCallback)
        {
            var backupObject = backupStream.DeserializeJson<BackupObject>();

			var progressInfo = new ExportImportProgressInfo();
            var totalCount = backupObject.CustomerOrders.Count();
            var take = 20;
            for (int skip = 0; skip < totalCount; skip += take)
            {
                _customerOrderService.SaveChanges(backupObject.CustomerOrders.ToArray());

                progressInfo.Description = String.Format("{0} of {1} orders imported", Math.Min(skip + take, totalCount), totalCount);
                progressCallback(progressInfo);
            }


            progressInfo.Description = String.Format("{0} orders importing", backupObject.CustomerOrders.Count());
			progressCallback(progressInfo);

            _customerOrderService.SaveChanges(backupObject.CustomerOrders.ToArray());
        }      

		private BackupObject GetBackupObject(Action<ExportImportProgressInfo> progressCallback)
        {
			var retVal = new BackupObject();
			var progressInfo = new ExportImportProgressInfo();

            var take = 20;
            
            var searchResponse = _customerOrderSearchService.SearchCustomerOrders(new CustomerOrderSearchCriteria { Take = 0, ResponseGroup = CustomerOrderResponseGroup.Default.ToString() });

            for(int skip = 0; skip < searchResponse.TotalCount; skip += take)
            {
                searchResponse = _customerOrderSearchService.SearchCustomerOrders(new CustomerOrderSearchCriteria {Skip = skip, Take = take, ResponseGroup = CustomerOrderResponseGroup.Full.ToString() });

                progressInfo.Description = String.Format("{0} of {1} orders loading", Math.Min(skip + take, searchResponse.TotalCount), searchResponse.TotalCount);
                progressCallback(progressInfo);
                retVal.CustomerOrders.AddRange(searchResponse.Results);
            }
        
            //Do not serialize shipment and payment methods
            var allPayments = retVal.CustomerOrders.SelectMany(x => x.InPayments);
            var allShipments = retVal.CustomerOrders.SelectMany(x => x.Shipments);
            foreach(var payment in allPayments)
            {
                payment.PaymentMethod = null;
            }
            foreach (var shipment in allShipments)
            {
                shipment.ShippingMethod = null;
            }
            return retVal;
        }   


    }
}