using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Extensions;
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
            // GetFilters() modifies Keyword
            criteria = criteria.CloneTyped();
            var filters = GetFilters(criteria);

            var request = new SearchRequest
            {
                SearchKeywords = criteria.Keyword,
                SearchFields = new[] { IndexDocumentExtensions.ContentFieldName },
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

            if (criteria.ObjectIds?.Any() == true)
            {
                result.Add(new IdsFilter { Values = criteria.ObjectIds });
            }

            if (criteria is CustomerOrderSearchCriteria orderSearchCriteria)
            {
                result.AddRange(GetPermanentFilters(orderSearchCriteria));
            }

            return result;
        }



        protected virtual IList<IFilter> GetPermanentFilters(CustomerOrderSearchCriteria criteria)
        {
            var result = new List<IFilter>();

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var parseResult = _searchPhraseParser.Parse(criteria.Keyword);
                criteria.Keyword = parseResult.Keyword;
                result.AddRange(parseResult.Filters);
            }

            if (criteria.ObjectIds != null)
            {
                result.Add(new IdsFilter { Values = criteria.ObjectIds });
            }

            if (!criteria.StoreIds.IsNullOrEmpty())
            {
                result.Add(FilterHelper.CreateTermFilter("storeid", criteria.StoreIds));
            }

            if (!criteria.Statuses.IsNullOrEmpty())
            {
                result.Add(FilterHelper.CreateTermFilter("status", criteria.Statuses));
            }

            if (criteria.StartDate.HasValue && criteria.EndDate.HasValue)
            {
                result.Add(FilterHelper.CreateDateRangeFilter("createddate", criteria.StartDate, null, true, true));
            }
            else if (criteria.StartDate.HasValue)
            {
                result.Add(FilterHelper.CreateDateRangeFilter("createddate", criteria.StartDate, null, true, true));
            }
            else if (criteria.EndDate.HasValue)
            {
                result.Add(FilterHelper.CreateDateRangeFilter("createddate", null, criteria.EndDate, true, true));
            }

            if (!criteria.CustomerIds.IsNullOrEmpty())
            {
                result.Add(FilterHelper.CreateTermFilter("customerid", criteria.CustomerIds));
            }

            if (!string.IsNullOrEmpty(criteria.EmployeeId))
            {
                result.Add(FilterHelper.CreateTermFilter("employeeid", criteria.EmployeeId));
            }

            if (!criteria.WithPrototypes)
            {
                result.Add(FilterHelper.CreateTermFilter("isprototype", "false"));
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
