using System.Collections.Generic;
using VirtoCommerce.Platform.Core.ProcessSettings;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class WkHtmlToPdfSettings : ProcessSettings
    {
        public WkHtmlToPdfSettings()
        {
            Arguments = new List<string> { "--dpi 300 ", "--print-media-type ", "--page-size A4 ", "--encoding \"utf - 8\" ", "--viewport-size \"1920x1080\" " };
        }

        public override string ToolName => "wkhtmltopdf";
        public override string ToolPath => base.ToolPath ?? this.GetToolPathViaManualInstallation(ToolName);
    }
}
