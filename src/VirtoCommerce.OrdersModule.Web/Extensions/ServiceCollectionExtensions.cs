using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Validators;
using VirtoCommerce.OrdersModule.Web.Validation;

namespace VirtoCommerce.OrdersModule.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddValidators(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IValidator<CustomerOrder>, CustomerOrderValidator>();
            serviceCollection.AddTransient<IValidator<PaymentIn>, PaymentInValidator>();
            serviceCollection.AddTransient<IValidator<OrderPaymentInfo>, CapturePaymentRequestValidator>();
        }
    }
}
