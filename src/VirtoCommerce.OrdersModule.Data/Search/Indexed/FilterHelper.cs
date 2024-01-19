using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
