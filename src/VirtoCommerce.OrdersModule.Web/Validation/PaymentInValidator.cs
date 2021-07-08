using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Model;

namespace VirtoCommerce.OrdersModule.Web.Validation
{
    public class PaymentInValidator : AbstractValidator<PaymentIn>
    {
        public PaymentInValidator()
        {
            RuleFor(payment => payment.OrganizationId).MaximumLength(64);
            RuleFor(payment => payment.OrganizationName).MaximumLength(255);

            RuleFor(payment => payment.CustomerId).NotNull().NotEmpty().MaximumLength(64);
            RuleFor(payment => payment.CustomerName).MaximumLength(255);

            RuleFor(payment => payment.Purpose).MaximumLength(1024);
            RuleFor(payment => payment.GatewayCode).MaximumLength(64);

            RuleFor(payment => payment.TaxType).MaximumLength(64);

            RuleFor(payment => payment.Price).ScalePrecision(4, 19);
            RuleFor(payment => payment.PriceWithTax).ScalePrecision(4, 19);

            RuleFor(payment => payment.DiscountAmount).ScalePrecision(4, 19);
            RuleFor(payment => payment.DiscountAmountWithTax).ScalePrecision(4, 19);

            RuleFor(payment => payment.Total).ScalePrecision(4, 19);
            RuleFor(payment => payment.TotalWithTax).ScalePrecision(4, 19);

            RuleFor(payment => payment.TaxTotal).ScalePrecision(4, 19);
        }
    }
}
