using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Data.Search.Indexed
{
    public static partial class FilterHelper
    {
        public static IFilter CreateTermFilter(string fieldName, string value)
        {
            return new TermFilter
            {
                FieldName = fieldName,
                Values = new[] { value },
            };
        }

        public static IFilter CreateTermFilter(string fieldName, IEnumerable<string> values)
        {
            return new TermFilter
            {
                FieldName = fieldName,
                Values = values.ToArray(),
            };
        }

        public static IFilter CreateDateRangeFilter(string fieldName, DateTime? lower, DateTime? upper, bool includeLower, bool includeUpper)
        {
            return CreateRangeFilter(fieldName, lower?.ToString("O"), upper?.ToString("O"), includeLower, includeUpper);
        }

        public static IFilter CreateRangeFilter(string fieldName, string lower, string upper, bool includeLower, bool includeUpper)
        {
            return new RangeFilter
            {
                FieldName = fieldName,
                Values = new[] { CreateRangeFilterValue(lower, upper, includeLower, includeUpper) },
            };
        }

        public static RangeFilterValue CreateRangeFilterValue(string lower, string upper, bool includeLower, bool includeUpper)
        {
            return new RangeFilterValue
            {
                Lower = lower,
                Upper = upper,
                IncludeLower = includeLower,
                IncludeUpper = includeUpper,
            };
        }

        #region Stringify
        [GeneratedRegex(@"^(?<fieldName>[A-Za-z0-9\-]+)(_[A-Za-z]{3})?$", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex FieldName();

        public static string Stringify(this IFilter filter)
        {
            return filter.Stringify(false);
        }

        public static string Stringify(this IFilter filter, bool skipCurrency)
        {
            var result = filter.ToString();
            switch (filter)
            {
                case RangeFilter rangeFilter:
                    {
                        var fieldName = rangeFilter.FieldName;
                        if (skipCurrency)
                        {
                            fieldName = FieldName().Match(fieldName).Groups["fieldName"].Value;
                        }
                        result = fieldName.Replace("_", "-") + "-" + string.Join("-", rangeFilter.Values.Select(Stringify));
                        break;
                    }

                case TermFilter termFilter:
                    result = termFilter.FieldName + (!termFilter.Values.IsNullOrEmpty() ? string.Join("_", termFilter.Values) : string.Empty);
                    break;
            }
            return result;
        }

        public static string Stringify(this RangeFilterValue rageValue)
        {
            if (rageValue.Lower == null && rageValue.Upper != null)
            {
                return $"under-{rageValue.Upper}";
            }
            else if (rageValue.Upper == null && rageValue.Lower != null)
            {
                return $"over-{rageValue.Lower}";
            }
            else
            {
                return $"{rageValue.Lower}-{rageValue.Upper}";
            }
        }
        #endregion

        #region SetAppliedAggregations
        /// <summary>
        /// Checks aggregation item values for the equality with the filters, and set <see cref="OrderAggregationItem.IsApplied"/> to those, whose value equal to  on of the filters
        /// </summary>
        /// <param name="searchRequest">Search request</param>
        /// <param name="aggregations">Calculated aggregation results</param>
        public static void SetAppliedAggregations(this SearchRequest searchRequest, IList<OrderAggregation> aggregations)
        {
            ArgumentNullException.ThrowIfNull(searchRequest);

            ArgumentNullException.ThrowIfNull(aggregations);

            foreach (var childFilter in searchRequest.GetChildFilters())
            {
                var aggregationItems = aggregations.Where(x => x.Field.EqualsIgnoreCase(childFilter.GetFieldName()))
                    .SelectMany(x => x.Items)
                    .ToArray();

                childFilter.FillIsAppliedForItems(aggregationItems);
            }
        }

        public static IList<IFilter> GetChildFilters(this SearchRequest searchRequest) =>
            (searchRequest?.Filter as AndFilter)?.ChildFilters ?? Array.Empty<IFilter>();

        public static string GetFieldName(this IFilter filter)
        {
            // TermFilter names are equal, RangeFilter can contain underscore in the name
            var fieldName = (filter as INamedFilter)?.FieldName;
            if (string.IsNullOrEmpty(fieldName))
            {
                return fieldName;
            }

            if (fieldName.StartsWith("__"))
            {
                return fieldName;
            }

            if (filter is RangeFilter)
            {
                return fieldName.Split('_')[0];
            }

            return fieldName;
        }

        public static void FillIsAppliedForItems(this IFilter filter, IEnumerable<OrderAggregationItem> aggregationItems)
        {
            foreach (var aggregationItem in aggregationItems)
            {
                switch (filter)
                {
                    case TermFilter termFilter:
                        // For term filters: just check result value in filter values
                        aggregationItem.IsApplied = termFilter.Values.Any(x => x.EqualsIgnoreCase(aggregationItem.Value?.ToString()));
                        break;
                    case RangeFilter rangeFilter:
                        // For range filters check the values have the same bounds
                        aggregationItem.IsApplied = rangeFilter.Values.Any(x =>
                            x.Lower.EqualsIgnoreCase(aggregationItem.RequestedLowerBound) && x.Upper.EqualsIgnoreCase(aggregationItem.RequestedUpperBound));
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion
    }
}
