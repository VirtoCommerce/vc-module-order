using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule2.Web.Validation;

namespace VirtoCommerce.OrdersModule2.Web.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddValidators(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IValidator<LineItem>, LineItemValidator>();
        }
    }
}
