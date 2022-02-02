using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.SearchModule.Core.Extenstions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.OrdersModule.Data.Search.Indexed
{
    public class CustomerOrderDocumentBuilder : IIndexDocumentBuilder
    {
        private readonly ICustomerOrderService _customerOrderService;
        private readonly IDynamicPropertySearchService _dynamicPropertySearchService;
        private readonly IStoreService _storeService;
        private readonly IFulfillmentCenterService _fulfillmentCenterService;

        public CustomerOrderDocumentBuilder(ICustomerOrderService customerOrderService, IDynamicPropertySearchService dynamicPropertySearchService, IStoreService storeService, IFulfillmentCenterService fulfillmentCenterService)
        {
            _customerOrderService = customerOrderService;
            _dynamicPropertySearchService = dynamicPropertySearchService;
            _storeService = storeService;
            _fulfillmentCenterService = fulfillmentCenterService;
        }

        public async Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            var result = new List<IndexDocument>();

            var orders = await _customerOrderService.GetByIdsAsync(documentIds.ToArray());

            foreach (var order in orders)
            {
                result.Add(await CreateDocument(order));
            }

            return result;
        }

        protected virtual async Task<IndexDocument> CreateDocument(CustomerOrder order)
        {
            var document = new IndexDocument(order.Id);

            document.AddFilterableAndSearchableValue("Number", order.Number);
            document.AddFilterableAndSearchableValue("EmployeeName", order.EmployeeName);
            document.AddFilterableAndSearchableValue("OrganizationName", order.OrganizationName);
            document.AddFilterableAndSearchableValue("CustomerName", order.CustomerName);
            document.AddFilterableAndSearchableValue("PurchaseOrderNumber", order.PurchaseOrderNumber);

            document.AddSearchableValue(order.Comment);

            document.AddFilterableValue("CreatedDate", order.CreatedDate, IndexDocumentFieldValueType.DateTime);
            document.AddFilterableValue("ModifiedDate", order.ModifiedDate ?? order.CreatedDate, IndexDocumentFieldValueType.DateTime);

            document.AddFilterableValue("CreatedBy", order.CreatedBy, IndexDocumentFieldValueType.String);
            document.AddFilterableValue("ModifiedBy", order.ModifiedBy, IndexDocumentFieldValueType.String);

            document.AddFilterableValue("CustomerId", order.CustomerId, IndexDocumentFieldValueType.String);
            document.AddFilterableValue("EmployeeId", order.EmployeeId, IndexDocumentFieldValueType.String);
            document.AddFilterableValue("OrganizationId", order.OrganizationId, IndexDocumentFieldValueType.String);
            document.AddFilterableValue("StoreId", order.StoreId, IndexDocumentFieldValueType.String);

            if (!order.StoreName.IsNullOrEmpty())
            {
                document.AddFilterableValue("StoreName", order.StoreName, IndexDocumentFieldValueType.String);
            }
            else if (!order.StoreId.IsNullOrEmpty())
            {
                var store = await _storeService.GetByIdAsync(order.StoreId, StoreResponseGroup.StoreInfo.ToString());
                document.AddFilterableValue("StoreName", store?.Name, IndexDocumentFieldValueType.String);
            }

            document.AddFilterableValue("OuterId", order.OuterId, IndexDocumentFieldValueType.String);
            document.AddFilterableValue("Status", order.Status, IndexDocumentFieldValueType.String);
            document.AddFilterableValue("Currency", order.Currency, IndexDocumentFieldValueType.String);
            document.AddFilterableValue("Total", order.Total, IndexDocumentFieldValueType.Double);
            document.AddFilterableValue("SubTotal", order.SubTotal, IndexDocumentFieldValueType.Double);
            document.AddFilterableValue("TaxTotal", order.TaxTotal, IndexDocumentFieldValueType.Double);
            document.AddFilterableValue("DiscountTotal", order.DiscountTotal, IndexDocumentFieldValueType.Double);
            document.AddFilterableValue("IsCancelled", order.IsCancelled, IndexDocumentFieldValueType.Boolean);

            foreach (var address in order.Addresses ?? Enumerable.Empty<Address>())
            {
                IndexAddress(address, document);
            }

            foreach (var lineItem in order.Items ?? Enumerable.Empty<LineItem>())
            {
                document.AddSearchableValue(lineItem.Comment);
            }

            foreach (var payment in order.InPayments ?? Enumerable.Empty<PaymentIn>())
            {
                IndexAddress(payment.BillingAddress, document);
                document.AddSearchableValue(payment.Number);
                document.AddSearchableValue(payment.Comment);
            }

            foreach (var shipment in order.Shipments ?? Enumerable.Empty<Shipment>())
            {
                IndexAddress(shipment.DeliveryAddress, document);
                document.AddSearchableValue(shipment.Number);
                document.AddSearchableValue(shipment.Comment);

                if (!shipment.FulfillmentCenterName.IsNullOrEmpty())
                {
                    document.AddSearchableValue(shipment.FulfillmentCenterName);
                }
                else if (!shipment.FulfillmentCenterId.IsNullOrEmpty())
                {
                    var fulfillmentCenter = (await _fulfillmentCenterService.GetByIdsAsync(new[] { shipment.FulfillmentCenterId }))
                        .FirstOrDefault();

                    document.AddSearchableValue(fulfillmentCenter?.Name);
                }
            }

            await IndexDynamicProperties(order, document);

            return document;
        }

        protected virtual void IndexAddress(Address address, IndexDocument document)
        {
            if (address != null)
            {
                document.AddSearchableValue($"{address.AddressType} {address}");
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
                        document.AddSearchableValue(value.ToString());
                    }
                }
            }

            // replace empty value for Boolean property with default 'False'
            if (property.ValueType == DynamicPropertyValueType.Boolean && values.IsNullOrEmpty())
            {
                document.Add(new IndexDocumentField(propertyName, false)
                {
                    IsRetrievable = true,
                    IsFilterable = true,
                    IsCollection = isCollection,
                    ValueType = property.ValueType.ToIndexedDocumentFieldValueType()
                });

                return;
            }

            if (!values.IsNullOrEmpty())
            {
                document.Add(new IndexDocumentField(propertyName, values)
                {
                    IsRetrievable = true,
                    IsFilterable = true,
                    IsCollection = isCollection,
                    ValueType = property.ValueType.ToIndexedDocumentFieldValueType()
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
