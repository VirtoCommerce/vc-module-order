using System;
using System.Collections.Generic;
using System.Globalization;
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
            var filter = GetFilters(criteria).And();
            var aggregations = GetAggregations(criteria);
            aggregations = ApplyMultiSelectFacetSearch(aggregations, filter);

            var request = new SearchRequest
            {
                SearchKeywords = criteria.Keyword,
                SearchFields = new[] { IndexDocumentExtensions.ContentFieldName },
                Filter = filter,
                Aggregations = aggregations,
                Sorting = GetSorting(criteria),
                Skip = criteria.Skip,
                Take = criteria.Take,
            };

            return Task.FromResult(request);
        }

        private IList<AggregationRequest> GetAggregations(SearchCriteriaBase criteria)
        {
            var result = new List<AggregationRequest>();

            if (criteria is not CustomerOrderIndexedSearchCriteria indexedSearchCriteria || string.IsNullOrEmpty(indexedSearchCriteria.Facet))
            {
                return result;
            }

            var facetExpessions = indexedSearchCriteria.Facet.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            result = facetExpessions
                .Select(x => new TermAggregationRequest
                {
                    FieldName = x,
                    Id = x,
                    Size = 0
                })
                .Cast<AggregationRequest>()
                .ToList();

            return result;
        }

        protected virtual IList<IFilter> GetFilters(SearchCriteriaBase criteria)
        {
            var result = new List<IFilter>();

            if (criteria.ObjectIds?.Any() == true)
            {
                result.Add(new IdsFilter { Values = criteria.ObjectIds });
            }

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var parseResult = _searchPhraseParser.Parse(criteria.Keyword);
                criteria.Keyword = parseResult.Keyword;
                result.AddRange(parseResult.Filters);
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

        public static IList<AggregationRequest> ApplyMultiSelectFacetSearch(IList<AggregationRequest> aggregations, IFilter filter)
        {
            foreach (var aggregation in aggregations ?? Array.Empty<AggregationRequest>())
            {
                var aggregationFilterFieldName = aggregation.FieldName ?? (aggregation.Filter as INamedFilter)?.FieldName;

                IList<IFilter> childFilters;
                var clonedFilter = (IFilter)filter.Clone();
                if (clonedFilter is AndFilter andFilter)
                {
                    childFilters = andFilter.ChildFilters;
                }
                else
                {
                    childFilters = new List<IFilter>() { clonedFilter };
                }

                // For multi-select facet mechanism, we should select
                // search request filters which do not have the same
                // names such as aggregation filter
                childFilters = childFilters
                    .Where(x =>
                    {
                        var result = true;

                        if (x is INamedFilter namedFilter)
                        {
                            result = !(aggregationFilterFieldName?.StartsWith(namedFilter.FieldName, true, CultureInfo.InvariantCulture) ?? false);
                        }

                        return result;
                    })
                    .ToList();

                aggregation.Filter = aggregation.Filter == null ? clonedFilter : aggregation.Filter.And(clonedFilter);
            }

            return aggregations;
        }
    }
}
