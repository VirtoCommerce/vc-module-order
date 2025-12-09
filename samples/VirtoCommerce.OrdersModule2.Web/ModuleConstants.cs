using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.OrdersModule2.Web
{
    public class ModuleConstants
    {
        public static class Permissions
        {
            public const string ReadPricesDirect = "order_samples:read_prices_direct";
            public const string ReadPricesIndirect = "order_samples:read_prices_indirect";

            public static string[] AllPermissions { get; } =
            [
                ReadPricesDirect,
                ReadPricesIndirect,
            ];
        }

        public static class Settings
        {
            public static class General
            {
                public static SettingDescriptor InvoiceStatus = new SettingDescriptor
                {
                    Name = "Invoice.Status",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Orders|General",
                    IsDictionary = true,
                    DefaultValue = "New",
                    AllowedValues = new[] { "New", "Pending", "Authorized", "Cancelled", "Sample2", "Sample3" }
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        yield return InvoiceStatus;
                    }
                }
            }
        }
    }
}
