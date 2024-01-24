using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Data.Search.Indexed
{
    public static class FilterHelper
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
                            fieldName = new Regex(@"^(?<fieldName>[A-Za-z0-9\-]+)(_[A-Za-z]{3})?$", RegexOptions.IgnoreCase).Match(fieldName).Groups["fieldName"].Value;
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
    }
}
