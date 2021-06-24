using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Extenstions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.OrdersModule.Data.Search.Indexed
{
    public class CustomerOrderSearchRequestBuilder : ISearchRequestBuilder
    {
        public string DocumentType { get; } = ModuleConstants.OrderIndexDocumentType;

        private readonly ISearchPhraseParser _searchPhraseParser;

        public CustomerOrderSearchRequestBuilder(ISearchPhraseParser searchPhraseParser)
        {
            _searchPhraseParser = searchPhraseParser;
        }

        public Task<SearchRequest> BuildRequestAsync(SearchCriteriaBase criteria)
        {
            var filters = GetFilters(criteria);

            var request = new SearchRequest
            {
                SearchKeywords = criteria.Keyword,
                SearchFields = new[] { IndexDocumentExtensions.SearchableFieldName },
                Filter = filters.And(),
                Sorting = GetSorting(criteria),
                Skip = criteria.Skip,
                Take = criteria.Take,
            };

            return Task.FromResult(request);
        }

        protected virtual IList<IFilter> GetFilters(SearchCriteriaBase criteria)
        {
            var result = new List<IFilter>();

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var parseResult = _searchPhraseParser.Parse(criteria.Keyword);
                criteria.Keyword = parseResult.Keyword;
                result.AddRange(parseResult.Filters);
            }

            if (criteria.ObjectIds?.Any() == true)
            {
                result.Add(new IdsFilter { Values = criteria.ObjectIds });
            }

            return result;
        }


        protected virtual IList<SortingField> GetSorting(SearchCriteriaBase criteria)
        {
            var result = new List<SortingField>();

            foreach (var sortInfo in criteria.SortInfos)
            {
                var fieldName = sortInfo.SortColumn.ToLowerInvariant();
                var isDescending = sortInfo.SortDirection == SortDirection.Descending;
                result.Add(new SortingField(fieldName, isDescending));
            }

            return result;
        }
    }
}
