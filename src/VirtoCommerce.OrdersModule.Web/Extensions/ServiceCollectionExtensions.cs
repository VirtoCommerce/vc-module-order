using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Validators;

namespace VirtoCommerce.OrdersModule.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the order FluentValidation validators as singletons — their rule trees are
        /// stateless after construction, so a single reused instance avoids rebuilding the rule tree
        /// on every resolve.
        /// </summary>
        /// <remarks>
        /// CustomerOrderValidator is a singleton and captures its child validators
        /// (IValidator&lt;LineItem&gt;, IValidator&lt;Shipment&gt;, IValidator&lt;IOperation&gt;) at
        /// construction via SetValidator. Any child validator a downstream module registers is therefore
        /// captured for the application lifetime: register such child validators as singletons and keep
        /// them free of scoped/transient dependencies (for example a scoped DbContext), otherwise the
        /// singleton captures them (a captive dependency). Enabling ValidateScopes / ValidateOnBuild on
        /// the host surfaces such misconfigurations at startup instead of failing intermittently at runtime.
        /// </remarks>
        public static void AddValidators(this IServiceCollection serviceCollection)
        {
            // Register operation-level validators
            serviceCollection.AddSingleton<IValidator<IOperation>, OrderDocumentCountValidator>();
            
            // Register entity-specific validators
            serviceCollection.AddSingleton<IValidator<CustomerOrder>, CustomerOrderValidator>();
            serviceCollection.AddSingleton<IValidator<PaymentIn>, PaymentInValidator>();
            serviceCollection.AddSingleton<IValidator<OrderPaymentInfo>, PaymentRequestValidator>();
        }
    }
}
