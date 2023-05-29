using Microsoft.Extensions.Configuration;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Core.Extensions
{
    public static class ConfigurationExtensions
    {
        public static bool IsOrderFullTextSearchEnabled(this IConfiguration configuration)
        {
            var value = configuration["Search:OrderFullTextSearchEnabled"];
            return value.TryParse(false);
        }

        public static bool IsDefaultPlatformInventoryHandlerV2(this IConfiguration configuration)
        {
            var value = configuration["InventoryWorkflow"];
            return value == "VirtoCommerce.OrdersModule.Data.Handlers.AdjustInventoryOrderChangedEventHandlerV2";
        }
    }
}
