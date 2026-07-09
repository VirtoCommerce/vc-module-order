using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Validators;

namespace VirtoCommerce.OrdersModule.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
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
