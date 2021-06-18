using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.SearchModule.Core.Extenstions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.OrdersModule.Data.Search.Indexed
{
    public class CustomerOrderDocumentBuilder : IIndexDocumentBuilder
    {
        private readonly ICustomerOrderService _customerOrderService;
        private readonly IDynamicPropertySearchService _dynamicPropertySearchService;

        public CustomerOrderDocumentBuilder(ICustomerOrderService customerOrderService, IDynamicPropertySearchService dynamicPropertySearchService)
        {
            _customerOrderService = customerOrderService;
            _dynamicPropertySearchService = dynamicPropertySearchService;
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

            document.AddSearchableValue(order.Comment);

            document.AddFilterableValue("CreatedDate", order.CreatedDate);
            document.AddFilterableValue("ModifiedDate", order.ModifiedDate ?? order.CreatedDate);

            document.AddFilterableValue("CreatedBy", order.CreatedBy);
            document.AddFilterableValue("ModifiedBy", order.ModifiedBy);

            document.AddFilterableValue("CustomerId", order.CustomerId);
            document.AddFilterableValue("EmployeeId", order.EmployeeId);
            document.AddFilterableValue("OrganizationId", order.OrganizationId);
            document.AddFilterableValue("StoreId", order.StoreId);
            document.AddFilterableValue("StoreName", order.StoreName);
            document.AddFilterableValue("OuterId", order.OuterId);
            document.AddFilterableValue("Status", order.Status);
            document.AddFilterableValue("Currency", order.Currency);
            document.AddFilterableValue("Total", order.Total);
            document.AddFilterableValue("SubTotal", order.SubTotal);
            document.AddFilterableValue("TaxTotal", order.TaxTotal);
            document.AddFilterableValue("DiscountTotal", order.DiscountTotal);
            document.AddFilterableValue("IsCancelled", order.IsCancelled);

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
                document.AddSearchableValue(shipment.FulfillmentCenterName);
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

        // TODO: PT-2562, handle null values correctly
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
            IList<string> shotTextValues = new List<string>();

            var isCollection = property.IsDictionary || property.IsArray;

            if (objectProperty != null)
            {
                values = objectProperty.Values.Where(x => x.Value != null)
                    .Select(x => x.Value)
                    .ToList();

                if (property.ValueType == DynamicPropertyValueType.ShortText)
                {
                    shotTextValues.AddRange(values.Cast<string>());
                }
            }

            // Use default or empty value for the property in index to be able to filter by it
            if (values.IsNullOrEmpty())
            {
                values = new[] { property.IsRequired
                        ? GetDynamicPropertyDefaultValue(property) ?? NoValueString
                        : NoValueString
                    };
            }

            document.Add(new IndexDocumentField(propertyName, values) { IsRetrievable = true, IsFilterable = true, IsCollection = isCollection });

            // add DynamicProperties that have the ShortText value type to __content
            foreach (var value in shotTextValues)
            {
                document.AddSearchableValue(value);
            }
        }

        private object GetDynamicPropertyDefaultValue(DynamicProperty property)
        {
            object result;

            switch (property.ValueType)
            {
                case DynamicPropertyValueType.ShortText:
                case DynamicPropertyValueType.Html:
                case DynamicPropertyValueType.LongText:
                case DynamicPropertyValueType.Image:
                    result = default(string);
                    break;

                case DynamicPropertyValueType.Integer:
                    result = default(int);
                    break;

                case DynamicPropertyValueType.Decimal:
                    result = default(decimal);
                    break;

                case DynamicPropertyValueType.DateTime:
                    result = default(DateTime);
                    break;

                case DynamicPropertyValueType.Boolean:
                    result = default(bool);
                    break;

                default:
                    result = default(object);
                    break;
            }

            return result;
        }

        private bool HasValuesOfType(DynamicObjectProperty objectProperty, DynamicPropertyValueType valueType)
        {
            return objectProperty.Values?.Any(x => x.ValueType == valueType) ??
                objectProperty.ValueType == valueType;
        }
    }
}
