using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VirtoCommerce.NotificationsModule.Core.Services;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Extensions;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Notifications;
using VirtoCommerce.OrdersModule.Core.Search.Indexed;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Authorization;
using VirtoCommerce.OrdersModule.Data.ExportImport;
using VirtoCommerce.OrdersModule.Data.Handlers;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.OrdersModule.Data.Search.Indexed;
using VirtoCommerce.OrdersModule.Data.Services;
using VirtoCommerce.OrdersModule.Data.Validation;
using VirtoCommerce.OrdersModule.Web.Authorization;
using VirtoCommerce.OrdersModule.Web.JsonConverters;
using VirtoCommerce.OrdersModule.Web.Validation;
using VirtoCommerce.Platform.Core.Bus;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Web
{
    public class Module : IModule, IExportSupport, IImportSupport, IHasConfiguration
    {
        public ManifestModuleInfo ModuleInfo { get; set; }
        public IConfiguration Configuration { get; set; }

        private IApplicationBuilder _appBuilder;

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<OrderDbContext>((provider, options) =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                options.UseSqlServer(configuration.GetConnectionString(ModuleInfo.Id) ?? configuration.GetConnectionString("VirtoCommerce"));
            });

            // Fluent Validators
            serviceCollection.AddTransient<IValidator<CustomerOrder>, CustomerOrderValidator>();
            serviceCollection.AddTransient<IValidator<OperationEntity>, OperationEntityValidator>();
            serviceCollection.AddTransient<IValidator<PaymentIn>, PaymentInValidator>();
            serviceCollection.AddTransient<IValidator<ShipmentEntity>, ShipmentEntityValidator>();

            serviceCollection.AddTransient<IOrderRepository, OrderRepository>();
            serviceCollection.AddTransient<Func<IOrderRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<IOrderRepository>());
            serviceCollection.AddTransient<ICustomerOrderSearchService, CustomerOrderSearchService>();
            serviceCollection.AddTransient<ICustomerOrderService, CustomerOrderService>();
            serviceCollection.AddTransient<IPaymentSearchService, PaymentSearchService>();
            serviceCollection.AddTransient<IPaymentService, PaymentService>();
            serviceCollection.AddTransient<ICustomerOrderBuilder, CustomerOrderBuilder>();
            serviceCollection.AddTransient<ICustomerOrderTotalsCalculator, DefaultCustomerOrderTotalsCalculator>();
            serviceCollection.AddTransient<OrderExportImport>();
            serviceCollection.AddTransient<AdjustInventoryOrderChangedEventHandler>();
            serviceCollection.AddTransient<CancelPaymentOrderChangedEventHandler>();
            serviceCollection.AddTransient<LogChangesOrderChangedEventHandler>();
            serviceCollection.AddTransient<IndexCustomerOrderChangedEventHandler>();
            //Register as scoped because we use UserManager<> as dependency in this implementation
            serviceCollection.AddScoped<SendNotificationsOrderChangedEventHandler>();
            serviceCollection.AddTransient<PolymorphicOperationJsonConverter>();

            serviceCollection.AddTransient<IAuthorizationHandler, OrderAuthorizationHandler>();

            serviceCollection.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            serviceCollection.AddOptions<HtmlToPdfOptions>().Configure<IConfiguration>((opts, config) =>
            {
                config.GetSection("HtmlToPdf").Bind(opts);
            });

            serviceCollection.AddTransient<IIndexedCustomerOrderSearchService, IndexedCustomerOrderSearchService>();

            if (Configuration.IsOrderFullTextSearchEnabled())
            {
                serviceCollection.AddTransient<CustomerOrderSearchRequestBuilder>();

                serviceCollection.AddSingleton<CustomerOrderChangesProvider>();
                serviceCollection.AddSingleton<CustomerOrderDocumentBuilder>();

                serviceCollection.AddSingleton(provider => new IndexDocumentConfiguration
                {
                    DocumentType = ModuleConstants.OrderIndexDocumentType,
                    DocumentSource = new IndexDocumentSource
                    {
                        ChangesProvider = provider.GetService<CustomerOrderChangesProvider>(),
                        DocumentBuilder = provider.GetService<CustomerOrderDocumentBuilder>(),
                    },
                });
            }
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            _appBuilder = appBuilder;

            var dynamicPropertyRegistrar = appBuilder.ApplicationServices.GetRequiredService<IDynamicPropertyRegistrar>();
            dynamicPropertyRegistrar.RegisterType<CustomerOrder>();
            dynamicPropertyRegistrar.RegisterType<PaymentIn>();
            dynamicPropertyRegistrar.RegisterType<Shipment>();
            dynamicPropertyRegistrar.RegisterType<LineItem>();

            var fullTextSearchEnabled = Configuration.IsOrderFullTextSearchEnabled();
            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            var settings = ModuleConstants.Settings.General.AllSettings;

            if (fullTextSearchEnabled)
            {
                //Register indexation settings
                settings = settings.Union(ModuleConstants.Settings.IndexationSettings);
            }

            settingsRegistrar.RegisterSettings(settings, ModuleInfo.Id);
            //Register store level settings
            settingsRegistrar.RegisterSettingsForType(ModuleConstants.Settings.StoreLevelSettings, nameof(Store));

            var permissionsProvider = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsProvider.RegisterPermissions(ModuleConstants.Security.Permissions.AllPermissions.Select(x =>
                new Permission()
                {
                    GroupName = "Orders",
                    ModuleId = ModuleInfo.Id,
                    Name = x
                }).ToArray());

            AbstractTypeFactory<PermissionScope>.RegisterType<OnlyOrderResponsibleScope>();
            AbstractTypeFactory<PermissionScope>.RegisterType<OrderSelectedStoreScope>();

            permissionsProvider.WithAvailabeScopesForPermissions(new[] {
                                                                        ModuleConstants.Security.Permissions.Read,
                                                                        ModuleConstants.Security.Permissions.Update,
                                                                        ModuleConstants.Security.Permissions.Delete,
                                                                        }, new OnlyOrderResponsibleScope(), new OrderSelectedStoreScope());


            var inProcessBus = appBuilder.ApplicationServices.GetService<IHandlerRegistrar>();
            inProcessBus.RegisterHandler<OrderChangedEvent>((message, token) => appBuilder.ApplicationServices.GetService<AdjustInventoryOrderChangedEventHandler>().Handle(message));
            inProcessBus.RegisterHandler<OrderChangedEvent>((message, token) => appBuilder.ApplicationServices.GetService<CancelPaymentOrderChangedEventHandler>().Handle(message));
            inProcessBus.RegisterHandler<OrderChangedEvent>((message, token) => appBuilder.ApplicationServices.GetService<LogChangesOrderChangedEventHandler>().Handle(message));
            inProcessBus.RegisterHandler<OrderChangedEvent>((message, token) => appBuilder.ApplicationServices.CreateScope().ServiceProvider.GetService<SendNotificationsOrderChangedEventHandler>().Handle(message));
            inProcessBus.RegisterHandler<OrderChangedEvent>((message, token) => appBuilder.ApplicationServices.GetService<IndexCustomerOrderChangedEventHandler>().Handle(message));

            if (fullTextSearchEnabled)
            {
                var searchRequestBuilderRegistrar = appBuilder.ApplicationServices.GetService<ISearchRequestBuilderRegistrar>();
                searchRequestBuilderRegistrar.Register(ModuleConstants.OrderIndexDocumentType, appBuilder.ApplicationServices.GetService<CustomerOrderSearchRequestBuilder>);
            }

            using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<OrderDbContext>();
                dbContext.Database.MigrateIfNotApplied(MigrationName.GetUpdateV2MigrationName(ModuleInfo.Id));
                dbContext.Database.EnsureCreated();
                dbContext.Database.Migrate();
            }

            var notificationRegistrar = appBuilder.ApplicationServices.GetService<INotificationRegistrar>();
            notificationRegistrar.RegisterNotification<CancelOrderEmailNotification>();
            notificationRegistrar.RegisterNotification<InvoiceEmailNotification>();
            notificationRegistrar.RegisterNotification<NewOrderStatusEmailNotification>();
            notificationRegistrar.RegisterNotification<OrderCreateEmailNotification>();
            notificationRegistrar.RegisterNotification<OrderPaidEmailNotification>();
            notificationRegistrar.RegisterNotification<OrderSentEmailNotification>();

            // enable polymorphic types in API controller methods
            var mvcJsonOptions = appBuilder.ApplicationServices.GetService<IOptions<MvcNewtonsoftJsonOptions>>();
            mvcJsonOptions.Value.SerializerSettings.Converters.Add(appBuilder.ApplicationServices.GetService<PolymorphicOperationJsonConverter>());
        }

        public void Uninstall()
        {
        }

        public async Task ExportAsync(Stream outStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback,
            ICancellationToken cancellationToken)
        {
            await _appBuilder.ApplicationServices.GetRequiredService<OrderExportImport>().DoExportAsync(outStream,
                progressCallback, cancellationToken);
        }

        public async Task ImportAsync(Stream inputStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback,
            ICancellationToken cancellationToken)
        {
            await _appBuilder.ApplicationServices.GetRequiredService<OrderExportImport>().DoImportAsync(inputStream,
                progressCallback, cancellationToken);
        }
    }
}
