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
using VirtoCommerce.SearchModule.Core.Exceptions;
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

        public IndexedCustomerOrderSearchService(ISearchRequestBuilderRegistrar searchRequestBuilderRegistrar, ISearchProvider searchProvider, ICustomerOrderService customerOrderService, IConfiguration configuration)
        {
            _searchRequestBuilderRegistrar = searchRequestBuilderRegistrar;
            _searchProvider = searchProvider;
            _customerOrderService = customerOrderService;
            _configuration = configuration;
        }

        public virtual async Task<CustomerOrderSearchResult> SearchCustomerOrdersAsync(CustomerOrderIndexedSearchCriteria criteria)
        {
            if (!_configuration.IsOrderFullTextSearchEnabled())
            {
                throw new SearchException("Indexed order search is disabled. To enable it add 'Search:OrderFullTextSearchEnabled' configuration key to app settings and set it to true.");
            }

            var requestBuilder = _searchRequestBuilderRegistrar.GetRequestBuilderByDocumentType(ModuleConstants.OrderIndexDocumentType);
            var request = await requestBuilder.BuildRequestAsync(criteria);

            var response = await _searchProvider.SearchAsync(ModuleConstants.OrderIndexDocumentType, request);

            var result = await ConvertResponseAsync(response, criteria);
            return result;
        }

        protected virtual async Task<CustomerOrderSearchResult> ConvertResponseAsync(SearchResponse response, CustomerOrderIndexedSearchCriteria criteria)
        {
            var result = AbstractTypeFactory<CustomerOrderSearchResult>.TryCreateInstance();

            if (response != null)
            {
                result.TotalCount = (int)response.TotalCount;
                result.Results = await ConvertDocumentsAsync(response.Documents, criteria);
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
                .Select(x => itemsMap.TryGetValue(x.Id, out var item) ? item : null)
                .Where(x => x != null)
                .ToArray();

            result.AddRange(orders);

            return result;
        }
    }
}
