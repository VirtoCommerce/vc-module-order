using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.MetadataModule.Core.Services;
using VirtoCommerce.MetadataModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.OrdersModule2.Web.Authorization;
using VirtoCommerce.OrdersModule2.Web.Extensions;
using VirtoCommerce.OrdersModule2.Web.Model;
using VirtoCommerce.OrdersModule2.Web.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.OrdersModule2.Web
{
    /// <summary>
    /// This module demostrates how to extend OrdersModule
    /// </summary>
    public class Module : IModule
    {
        public ManifestModuleInfo ModuleInfo { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<Order2DbContext>((provider, options) =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                options.UseSqlServer(configuration.GetConnectionString(ModuleInfo.Id)
                                  ?? configuration.GetConnectionString("VirtoCommerce.Orders")
                                  ?? configuration.GetConnectionString("VirtoCommerce"));
            });

            serviceCollection.AddTransient<IOrderRepository, OrderRepository2>();
            serviceCollection.AddTransient<IAuthorizationHandler, CustomOrderAuthorizationHandler>();

            serviceCollection.AddValidators();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.General.AllSettings, ModuleInfo.Id);

            AbstractTypeFactory<PermissionScope>.RegisterType<OrderSelectedStatusScope>();

            var permissionsProvider = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsProvider.WithAvailabeScopesForPermissions(new[]
            {
                // Permission list
                OrdersModule.Core.ModuleConstants.Security.Permissions.Read
            }, new OrderSelectedStatusScope());

            AbstractTypeFactory<IOperation>.OverrideType<CustomerOrder, CustomerOrder2>();
            AbstractTypeFactory<CustomerOrderEntity>.OverrideType<CustomerOrderEntity, CustomerOrder2Entity>();
            AbstractTypeFactory<CustomerOrder>.OverrideType<CustomerOrder, CustomerOrder2>()
                .WithFactory(() => new CustomerOrder2 { OperationType = "CustomerOrder" });

            //Thats need for PolymorphicOperationJsonConverter for API deserialization
            AbstractTypeFactory<IOperation>.RegisterType<Invoice>();

            using var serviceScope = appBuilder.ApplicationServices.CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<Order2DbContext>();
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();



            /// --------------------- Check metadata service below

            var metadataService = appBuilder.ApplicationServices.GetRequiredService<IMetadataService>();

            // All order properties projection (full, no details)
            var projectionDefault = new Projection(typeof(CustomerOrder), ReadType.OnlyThat)
            {
                Name = "Default"
            };
            metadataService.AddProjection(projectionDefault);

            // All order properties projection (full + 1st level details)
            var projectionDefaultDetailsRelated = new Projection(typeof(CustomerOrder), ReadType.RelatedDetails)
            {
                Name = "DefaultDetailsRelated1stLevel"
            };
            metadataService.AddProjection(projectionDefaultDetailsRelated);

            // An example with main order info (as example -- for orders list)
            var projectionShortInfoList = new Projection
            {
                DefineClassType = typeof(CustomerOrder),
                Name = "ShortInfoList"
            };

            projectionShortInfoList.AddProperties(new string[] {
                nameof(CustomerOrder.StoreId),
                nameof(CustomerOrder.Id),
                nameof(CustomerOrder.CustomerName),
                nameof(CustomerOrder.OrganizationName),
                nameof(CustomerOrder.CreatedDate),
                nameof(CustomerOrder.CreatedBy),
                nameof(CustomerOrder.Sum),
                nameof(CustomerOrder.Status)
            });
            metadataService.AddProjection(projectionShortInfoList);

            // An example with main order info (as example -- payments edit)
            var projectionWithLineItems = new Projection
            {
                DefineClassType = typeof(CustomerOrder),
                Name = "WithLineItems"
            };
            projectionWithLineItems.AddProperties(new string[] {
                nameof(CustomerOrder.StoreId),
                nameof(CustomerOrder.Id),
                nameof(CustomerOrder.CustomerName),
                nameof(CustomerOrder.OrganizationName),
                nameof(CustomerOrder.CreatedDate),
                nameof(CustomerOrder.CreatedBy),
                nameof(CustomerOrder.Sum),
                nameof(CustomerOrder.Status),
                nameof(CustomerOrder.Currency),
                nameof(CustomerOrder.Total),
            });
            projectionWithLineItems.AddDetailInProjection(nameof(CustomerOrder.Items), new Projection(typeof(LineItem), ReadType.OnlyThat), true);
            projectionWithLineItems.AddDetailInProjection(nameof(CustomerOrder.InPayments), new Projection(typeof(PaymentIn), ReadType.OnlyThat), true);

            metadataService.AddProjection(projectionWithLineItems);

        }

        public void Uninstall()
        {
            // No needed
        }
    }
}
