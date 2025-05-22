using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.OrdersModule.Data.Search.Indexed
{
    public class CustomerOrderDocumentBuilder : IIndexSchemaBuilder, IIndexDocumentBuilder
    {
        private readonly ICustomerOrderService _customerOrderService;
        private readonly IDynamicPropertySearchService _dynamicPropertySearchService;
        private readonly IStoreService _storeService;
        private readonly IFulfillmentCenterService _fulfillmentCenterService;

        public CustomerOrderDocumentBuilder(
            ICustomerOrderService customerOrderService,
            IDynamicPropertySearchService dynamicPropertySearchService,
            IStoreService storeService,
            IFulfillmentCenterService fulfillmentCenterService)
        {
            _customerOrderService = customerOrderService;
            _dynamicPropertySearchService = dynamicPropertySearchService;
            _storeService = storeService;
            _fulfillmentCenterService = fulfillmentCenterService;
        }

        public Task BuildSchemaAsync(IndexDocument schema)
        {
            schema.AddFilterableStringAndContentString("Number");
            schema.AddFilterableStringAndContentString("EmployeeName");
            schema.AddFilterableStringAndContentString("OrganizationName");
            schema.AddFilterableStringAndContentString("CustomerName");
            schema.AddFilterableStringAndContentString("PurchaseOrderNumber");

            schema.AddFilterableDateTime("CreatedDate");
            schema.AddFilterableDateTime("ModifiedDate");

            schema.AddFilterableString("CreatedBy");
            schema.AddFilterableString("ModifiedBy");
            schema.AddFilterableString("CustomerId");
            schema.AddFilterableString("EmployeeId");
            schema.AddFilterableString("OrganizationId");
            schema.AddFilterableString("StoreId");
            schema.AddFilterableString("StoreName");
            schema.AddFilterableString("OuterId");
            schema.AddFilterableString("Status");
            schema.AddFilterableString("Currency");

            schema.AddFilterableDecimal("Total");
            schema.AddFilterableDecimal("SubTotal");
            schema.AddFilterableDecimal("TaxTotal");
            schema.AddFilterableDecimal("DiscountTotal");

            schema.AddFilterableBoolean("IsCancelled");
            schema.AddFilterableBoolean("IsPrototype");

            return schema.AddDynamicProperties(_dynamicPropertySearchService, typeof(CustomerOrder).FullName);
        }

        public async Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            var result = new List<IndexDocument>();

            var orders = await _customerOrderService.GetNoCloneAsync(documentIds);

            foreach (var order in orders)
            {
                result.Add(await CreateDocument(order));
            }

            return result;
        }

        protected virtual async Task<IndexDocument> CreateDocument(CustomerOrder order)
        {
            var document = new IndexDocument(order.Id);

            document.AddFilterableStringAndContentString("Number", order.Number);
            document.AddFilterableStringAndContentString("EmployeeName", order.EmployeeName);
            document.AddFilterableStringAndContentString("OrganizationName", order.OrganizationName);
            document.AddFilterableStringAndContentString("CustomerName", order.CustomerName);
            document.AddFilterableStringAndContentString("PurchaseOrderNumber", order.PurchaseOrderNumber);

            document.AddContentString(order.Comment);

            document.AddFilterableDateTime("CreatedDate", order.CreatedDate);
            document.AddFilterableDateTime("ModifiedDate", order.ModifiedDate ?? order.CreatedDate);

            document.AddFilterableString("CreatedBy", order.CreatedBy);
            document.AddFilterableString("ModifiedBy", order.ModifiedBy);

            document.AddFilterableString("CustomerId", order.CustomerId);
            document.AddFilterableString("EmployeeId", order.EmployeeId);
            document.AddFilterableString("OrganizationId", order.OrganizationId);
            document.AddFilterableString("StoreId", order.StoreId);

            if (!order.StoreName.IsNullOrEmpty())
            {
                document.AddFilterableString("StoreName", order.StoreName);
            }
            else if (!order.StoreId.IsNullOrEmpty())
            {
                var store = await _storeService.GetNoCloneAsync(order.StoreId, StoreResponseGroup.StoreInfo.ToString());
                document.AddFilterableString("StoreName", store?.Name);
            }

            document.AddFilterableString("OuterId", order.OuterId);
            document.AddFilterableString("Status", order.Status);
            document.AddFilterableString("Currency", order.Currency);

            document.AddFilterableDecimal("Total", order.Total);
            document.AddFilterableDecimal("SubTotal", order.SubTotal);
            document.AddFilterableDecimal("TaxTotal", order.TaxTotal);
            document.AddFilterableDecimal("DiscountTotal", order.DiscountTotal);

            document.AddFilterableBoolean("IsCancelled", order.IsCancelled);
            document.AddFilterableBoolean("IsPrototype", order.IsPrototype);

            foreach (var address in order.Addresses ?? Enumerable.Empty<Address>())
            {
                IndexAddress(address, document);
            }

            foreach (var lineItem in order.Items ?? Enumerable.Empty<LineItem>())
            {
                document.AddContentString(lineItem.Comment);
            }

            foreach (var payment in order.InPayments ?? Enumerable.Empty<PaymentIn>())
            {
                IndexAddress(payment.BillingAddress, document);
                document.AddContentString(payment.Number);
                document.AddContentString(payment.Comment);
            }

            foreach (var shipment in order.Shipments ?? Enumerable.Empty<Shipment>())
            {
                IndexAddress(shipment.DeliveryAddress, document);
                document.AddContentString(shipment.Number);
                document.AddContentString(shipment.Comment);

                if (!shipment.FulfillmentCenterName.IsNullOrEmpty())
                {
                    document.AddContentString(shipment.FulfillmentCenterName);
                }
                else if (!shipment.FulfillmentCenterId.IsNullOrEmpty())
                {
                    var fulfillmentCenter = await _fulfillmentCenterService.GetNoCloneAsync(shipment.FulfillmentCenterId);
                    document.AddContentString(fulfillmentCenter?.Name);
                }
            }

#pragma warning disable VC0005 // Type or member is obsolete
            await IndexDynamicProperties(order, document);
#pragma warning restore VC0005 // Type or member is obsolete

            return document;
        }

        protected virtual void IndexAddress(Address address, IndexDocument document)
        {
            if (address != null)
            {
                document.AddContentString($"{address.AddressType} {address}");
            }
        }

        [Obsolete("Will be deleted in stable bundle 7", DiagnosticId = "VC0005", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        public static readonly string NoValueString = "__null";

        [Obsolete("Use IndexDocument.AddDynamicProperties() extension method", DiagnosticId = "VC0005", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        protected virtual async Task IndexDynamicProperties(CustomerOrder order, IndexDocument document)
        {
            var properties = await _dynamicPropertySearchService.GetAllDynamicProperties(order.ObjectType);

            foreach (var property in properties)
            {
                var objectProperty = order.DynamicProperties?.FirstOrDefault(x => x.Id == property.Id) ??
                     order.DynamicProperties?.FirstOrDefault(x => x.Name.EqualsIgnoreCase(property.Name) && x.HasValuesOfType(property.ValueType));

                IndexDynamicProperty(document, property, objectProperty);
            }
        }

        [Obsolete("Use IndexDocument.AddDynamicProperty() extension method", DiagnosticId = "VC0005", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        protected virtual void IndexDynamicProperty(IndexDocument document, DynamicProperty property, DynamicObjectProperty objectProperty)
        {
            document.AddDynamicProperty(property, objectProperty);
        }
    }
}
