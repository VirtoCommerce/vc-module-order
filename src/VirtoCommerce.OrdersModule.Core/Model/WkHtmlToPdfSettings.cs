using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.ProcessSettings;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class WkHtmlToPdfSettings : ProcessSettings
    {
        public WkHtmlToPdfSettings(PlatformOptions platformOptions) : base(platformOptions)
        {
        }

        public override string ToolName => "wkhtmltopdf";
    }
}
