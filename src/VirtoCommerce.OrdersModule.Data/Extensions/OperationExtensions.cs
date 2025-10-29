using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VirtoCommerce.CoreModule.Core.Common;

namespace VirtoCommerce.OrdersModule.Data.Extensions
{
    public static class OperationExtensions
    {
        [Obsolete("Use VirtoCommerce.OrdersModule.Core.Extensions.OperationExtensions.FillChildOperations()", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        public static void FillAllChildOperations(this IOperation operation)
        {
            var retVal = new List<IOperation>();
            var objectType = operation.GetType();

            var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var childOperations = properties.Where(x => x.PropertyType.GetInterface(typeof(IOperation).Name) != null)
                .Select(x => (IOperation)x.GetValue(operation)).Where(x => x != null).ToList();

            foreach (var childOperation in childOperations)
            {
                retVal.Add(childOperation);
            }

            //Handle collection and arrays
            var collections = properties.Where(p => p.GetIndexParameters().Length == 0)
                .Select(x => x.GetValue(operation, null))
                .Where(x => x is IEnumerable && !(x is string))
                .Cast<IEnumerable>();

            foreach (var collection in collections)
            {
                foreach (var childOperation in collection.OfType<IOperation>())
                {
                    retVal.Add(childOperation);
                }
            }

            operation.ChildrenOperations = retVal;
        }
    }
}
