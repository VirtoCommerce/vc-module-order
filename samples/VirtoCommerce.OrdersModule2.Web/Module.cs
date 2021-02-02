using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.OrdersModule2.Web.Model;
using VirtoCommerce.OrdersModule2.Web.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.OrdersModule2.Web
{
    public class Module : IModule
    {
        public ManifestModuleInfo ModuleInfo { get; set; }
        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<Order2DbContext>((provider, options) =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                options.UseSqlServer(configuration.GetConnectionString(ModuleInfo.Id) ?? configuration.GetConnectionString("VirtoCommerce.Orders") ?? configuration.GetConnectionString("VirtoCommerce"));
            });
            serviceCollection.AddTransient<IOrderRepository, OrderRepository2>();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.General.AllSettings, ModuleInfo.Id);

            AbstractTypeFactory<IOperation>.OverrideType<CustomerOrder, CustomerOrder2>();
            AbstractTypeFactory<CustomerOrderEntity>.OverrideType<CustomerOrderEntity, CustomerOrder2Entity>();
            AbstractTypeFactory<CustomerOrder>.OverrideType<CustomerOrder, CustomerOrder2>()
                .WithFactory(() => new CustomerOrder2 { OperationType = "CustomerOrder" });
            //Thats need for PolymorphicOperationJsonConverter for API deserialization
            AbstractTypeFactory<IOperation>.RegisterType<Invoice>();

            using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<Order2DbContext>();
                dbContext.Database.EnsureCreated();
                dbContext.Database.Migrate();
            }
        }

        public void Uninstall()
        {
        }
    }
}
