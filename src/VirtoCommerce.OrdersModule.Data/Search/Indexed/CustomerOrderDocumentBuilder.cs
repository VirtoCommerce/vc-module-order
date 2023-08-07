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

            return Task.CompletedTask;
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

            await IndexDynamicProperties(order, document);

            return document;
        }

        protected virtual void IndexAddress(Address address, IndexDocument document)
        {
            if (address != null)
            {
                document.AddContentString($"{address.AddressType} {address}");
            }
        }

        // PT-2562: handle null values correctly
        public static readonly string NoValueString = "__null";

        protected virtual async Task IndexDynamicProperties(CustomerOrder order, IndexDocument document)
        {
            var criteria = AbstractTypeFactory<DynamicPropertySearchCriteria>.TryCreateInstance();
            criteria.ObjectTypes = new[] { order.ObjectType };
            criteria.Take = int.MaxValue;

            var searchResult = await _dynamicPropertySearchService.SearchDynamicPropertiesAsync(criteria);
            var typeDynamicProperties = searchResult.Results;

            if (typeDynamicProperties.IsNullOrEmpty())
            {
                return;
            }

            foreach (var property in typeDynamicProperties)
            {
                var objectProperty = order.DynamicProperties?.FirstOrDefault(x => x.Id == property.Id) ??
                     order.DynamicProperties?.FirstOrDefault(x => x.Name.EqualsInvariant(property.Name) && HasValuesOfType(x, property.ValueType));

                IndexDynamicProperty(document, property, objectProperty);
            }
        }

        protected virtual void IndexDynamicProperty(IndexDocument document, DynamicProperty property, DynamicObjectProperty objectProperty)
        {
            var propertyName = property.Name?.ToLowerInvariant();

            IList<object> values = null;

            var isCollection = property.IsDictionary || property.IsArray;

            if (objectProperty != null)
            {
                values = objectProperty.Values.Where(x => x.Value != null)
                    .Select(x => x.Value)
                    .ToList();

                // add DynamicProperties that have the ShortText value type to __content
                if (property.ValueType == DynamicPropertyValueType.ShortText)
                {
                    foreach (var value in values)
                    {
                        document.AddContentString(value.ToString());
                    }
                }
            }

            // replace empty value for Boolean property with default 'False'
            if (property.ValueType == DynamicPropertyValueType.Boolean && values.IsNullOrEmpty())
            {
                document.Add(new IndexDocumentField(propertyName, false, IndexDocumentFieldValueType.Boolean)
                {
                    IsRetrievable = true,
                    IsFilterable = true,
                    IsCollection = isCollection,
                });

                return;
            }

            if (!values.IsNullOrEmpty())
            {
                document.Add(new IndexDocumentField(propertyName, values, property.ValueType.ToIndexedDocumentFieldValueType())
                {
                    IsRetrievable = true,
                    IsFilterable = true,
                    IsCollection = isCollection,
                });
            }
        }

        private bool HasValuesOfType(DynamicObjectProperty objectProperty, DynamicPropertyValueType valueType)
        {
            return objectProperty.Values?.Any(x => x.ValueType == valueType) ??
                objectProperty.ValueType == valueType;
        }
    }
}
