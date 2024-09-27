using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Extensions;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.OrdersModule.Core.Search.Indexed;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Exceptions;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.OrdersModule.Data.Search.Indexed
{
    public class IndexedCustomerOrderSearchService : IIndexedCustomerOrderSearchService
    {
        private readonly ISearchRequestBuilderRegistrar _searchRequestBuilderRegistrar;
        private readonly ISearchProvider _searchProvider;
        private readonly ICustomerOrderService _customerOrderService;
        private readonly IConfiguration _configuration;
        private readonly ILocalizableSettingService _localizableSettingService;

        private readonly Dictionary<string, string> _fieldBySettingName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "status", ModuleConstants.Settings.General.OrderStatus.Name }
        };

        public IndexedCustomerOrderSearchService(
            ISearchRequestBuilderRegistrar searchRequestBuilderRegistrar,
            ISearchProvider searchProvider,
            ICustomerOrderService customerOrderService,
            IConfiguration configuration,
            ILocalizableSettingService localizableSettingService)
        {
            _searchRequestBuilderRegistrar = searchRequestBuilderRegistrar;
            _searchProvider = searchProvider;
            _customerOrderService = customerOrderService;
            _configuration = configuration;
            _localizableSettingService = localizableSettingService;
        }

        public virtual async Task<CustomerOrderIndexedSearchResult> SearchCustomerOrdersAsync(CustomerOrderIndexedSearchCriteria criteria)
        {
            if (!_configuration.IsOrderFullTextSearchEnabled())
            {
                throw new SearchException("Indexed order search is disabled. To enable it add 'Search:OrderFullTextSearchEnabled' configuration key to app settings and set it to true.");
            }

            var requestBuilder = _searchRequestBuilderRegistrar.GetRequestBuilderByDocumentType(ModuleConstants.OrderIndexDocumentType);
            var request = await requestBuilder.BuildRequestAsync(criteria);

            var response = await _searchProvider.SearchAsync(ModuleConstants.OrderIndexDocumentType, request);

            var result = await ConvertResponseAsync(response, criteria, request);
            return result;
        }

        protected virtual async Task<CustomerOrderIndexedSearchResult> ConvertResponseAsync(SearchResponse response, CustomerOrderIndexedSearchCriteria criteria, SearchRequest searchRequest)
        {
            var result = AbstractTypeFactory<CustomerOrderIndexedSearchResult>.TryCreateInstance();

            if (response != null)
            {
                result.TotalCount = (int)response.TotalCount;
                result.Results = await ConvertDocumentsAsync(response.Documents, criteria);
                result.Aggregations = await ConvertAggregationsAsync(response.Aggregations, searchRequest, criteria);
            }

            return result;
        }

        protected virtual async Task<IList<CustomerOrder>> ConvertDocumentsAsync(IList<SearchDocument> documents, CustomerOrderIndexedSearchCriteria criteria)
        {
            var result = new List<CustomerOrder>();

            if (documents?.Any() != true)
            {
                return result;
            }

            var itemIds = documents.Select(x => x.Id).ToArray();
            var items = await _customerOrderService.GetAsync(itemIds, criteria.ResponseGroup);
            var itemsMap = items.ToDictionary(x => x.Id, x => x);

            // Preserve documents order
            var orders = documents
                .Select(doc =>
                {
                    var order = itemsMap.TryGetValue(doc.Id, out var value) ? value : null;

                    if (order != null)
                    {
                        order.RelevanceScore = doc.GetRelevanceScore();
                    }

                    return order;
                })
                .Where(x => x != null)
                .ToArray();

            result.AddRange(orders);

            return result;
        }

        private async Task<IList<OrderAggregation>> ConvertAggregationsAsync(IList<AggregationResponse> aggregationResponses, SearchRequest searchRequest, CustomerOrderIndexedSearchCriteria criteria)
        {
            var result = new List<OrderAggregation>();

            foreach (var aggregationRequest in searchRequest.Aggregations)
            {
                var aggregationResponse = aggregationResponses.FirstOrDefault(x => x.Id == aggregationRequest.Id);
                if (aggregationResponse != null)
                {
                    var orderAggregation = default(OrderAggregation);

                    if (aggregationRequest is RangeAggregationRequest rangeAggregationRequest)
                    {
                        orderAggregation = new OrderAggregation()
                        {
                            AggregationType = "range",
                            Field = aggregationRequest.FieldName,
                            Items = GetAttributeAggregationItems(rangeAggregationRequest, aggregationResponse.Values),
                        };
                    }
                    else if (aggregationRequest is TermAggregationRequest)
                    {
                        orderAggregation = new OrderAggregation()
                        {
                            AggregationType = "attr",
                            Field = aggregationRequest.FieldName,
                            Items = await GetAttributeAggregationItemsAsync(aggregationRequest.FieldName, criteria.LanguageCode, aggregationResponse.Values),
                        };
                    }

                    result.Add(orderAggregation);
                }
            }

            searchRequest.SetAppliedAggregations(result);

            return result;
        }

        private static List<OrderAggregationItem> GetAttributeAggregationItems(RangeAggregationRequest rangeAggregationRequest, IList<AggregationResponseValue> resultValues)
        {
            var result = new List<OrderAggregationItem>();

            foreach (var requestValue in rangeAggregationRequest.Values)
            {
                var resultValue = resultValues.FirstOrDefault(x => x.Id == requestValue.Id);
                if (resultValue != null)
                {
                    var aggregationItem = new OrderAggregationItem
                    {
                        Value = resultValue.Id,
                        Count = (int)resultValue.Count,
                        RequestedLowerBound = requestValue.Lower,
                        RequestedUpperBound = requestValue.Upper,
                        IncludeLower = requestValue.IncludeLower,
                        IncludeUpper = requestValue.IncludeUpper,
                    };

                    result.Add(aggregationItem);
                }
            }

            return result;
        }

        private async Task<IList<OrderAggregationItem>> GetAttributeAggregationItemsAsync(string fieldName, string languageCode, IList<AggregationResponseValue> aggregationResponseValues)
        {
            IList<KeyValue> localizedValues = null;
            if (!string.IsNullOrEmpty(languageCode) && _fieldBySettingName.TryGetValue(fieldName, out var settingName))
            {
                localizedValues = await _localizableSettingService.GetValuesAsync(ModuleConstants.Settings.General.OrderStatus.Name, languageCode);
            }

            var result = aggregationResponseValues
                .Select(x =>
                {
                    var item = new OrderAggregationItem
                    {
                        Value = x.Id,
                        Count = (int)x.Count,
                    };

                    if (!string.IsNullOrEmpty(languageCode) && !localizedValues.IsNullOrEmpty())
                    {
                        var localizedValue = localizedValues.FirstOrDefault(y => y.Key.EqualsInvariant(x.Id));

                        if (localizedValue != null)
                        {
                            var label = new OrderAggregationLabel
                            {
                                Language = languageCode,
                                Label = localizedValue.Value,
                            };

                            item.Labels = new List<OrderAggregationLabel> { label };
                        }
                    }

                    return item;
                })
                .ToList();

            return result;
        }
    }
}
