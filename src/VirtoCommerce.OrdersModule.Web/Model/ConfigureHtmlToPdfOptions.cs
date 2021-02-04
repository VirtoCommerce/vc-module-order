using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Web.Model
{
    // The workaround class to escape calling serviceCollection.BuildServiceProvider(), based on https://andrewlock.net/access-services-inside-options-and-startup-using-configureoptions/#the-new-improved-answer
    public class ConfigureHtmlToPdfOptions : IConfigureOptions<HtmlToPdfOptions>
    {
        private readonly IConfiguration _configuration;

        public ConfigureHtmlToPdfOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(HtmlToPdfOptions options)
        {
            _configuration.GetSection("HtmlToPdf").Bind(options);
        }
    }
}
