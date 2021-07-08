using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Web.Validation
{
    public static class OrderOperationExtension
    {
        public static Task<ValidationResult> ValidateAsync(this OrderOperation orderOperation) => orderOperation switch
        {
            CustomerOrder order => new CustomerOrderValidator().ValidateAsync(order),
            PaymentIn payment => new PaymentInValidator().ValidateAsync(payment),
            _ => throw new NotImplementedException("Need to implement OrderOperationValidator")
        };
    }
}
