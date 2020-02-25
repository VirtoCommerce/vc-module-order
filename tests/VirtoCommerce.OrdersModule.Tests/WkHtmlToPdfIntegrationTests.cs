using System.IO;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.ProcessSettings;
using VirtoCommerce.Platform.Data.Helpers;
using Xunit;

namespace VirtoCommerce.OrdersModule.Tests
{
    [Trait("Category", "IntegrationTest")]
    public class ProcessHelperIntegrationTests
    {
        /// <summary>
        /// need to add input.html in current directory when running the test
        /// and set the Platform target x86/x64 in the build options
        /// </summary>
        [Fact]
        public void StartProcess_ConvertHtmlToPdf()
        {
            var result = ProcessHelper.StartProcess(new WkHtmlToPdfSettings()
                    .SetWorkingDirectory(Directory.GetCurrentDirectory())
                    .SetArguments(new[] { "input.html", " ", " - " }))
                .GetOutputAsByteArray();

            File.WriteAllBytes("output.pdf", result);
        }
    }
}
