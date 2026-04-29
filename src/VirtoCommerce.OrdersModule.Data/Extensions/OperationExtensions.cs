using System;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.OrdersModule.Core.Extensions;

namespace VirtoCommerce.OrdersModule.Data.Extensions
{
    public static class OperationExtensions
    {
        [Obsolete("Use VirtoCommerce.OrdersModule.Core.Extensions.OperationExtensions.FillChildOperations()", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        public static void FillAllChildOperations(this IOperation operation)
        {
            operation.FillChildOperations();
        }
    }
}
