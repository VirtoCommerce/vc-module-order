using System.Collections;
using System.Linq;
using System.Reflection;
using VirtoCommerce.CoreModule.Core.Common;

namespace VirtoCommerce.OrdersModule.Core.Extensions;

public static class OperationExtensions
{
    public static void FillChildOperations(this IOperation operation)
    {
        var properties = operation.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var childOperations = properties
            .Where(x => x.PropertyType.GetInterface(nameof(IOperation)) != null)
            .Select(x => (IOperation)x.GetValue(operation)).Where(x => x != null)
            .ToList();

        // Handle collections
        var collections = properties
            .Where(x => x.Name != nameof(operation.ChildrenOperations) && x.GetIndexParameters().Length == 0)
            .Select(x => x.GetValue(operation, index: null))
            .Where(x => x is IEnumerable and not string)
            .Cast<IEnumerable>();

        foreach (var collection in collections)
        {
            childOperations.AddRange(collection.OfType<IOperation>());
        }

        operation.ChildrenOperations = childOperations;
    }
}
