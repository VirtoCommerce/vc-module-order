using FluentValidation;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Data.Validators;

public class PaymentInValidator : AbstractValidator<PaymentIn>
{
    public PaymentInValidator()
    {
        SetDefaultRules();
    }

    protected void SetDefaultRules()
    {
        RuleFor(payment => payment.OrganizationId).MaximumLength(64);
        RuleFor(payment => payment.OrganizationName).MaximumLength(255);
        RuleFor(payment => payment.CustomerId).NotNull().NotEmpty().MaximumLength(64);
        RuleFor(payment => payment.CustomerName).MaximumLength(255);
        RuleFor(payment => payment.Purpose).MaximumLength(1024);
        RuleFor(payment => payment.GatewayCode).MaximumLength(64);
        RuleFor(payment => payment.TaxType).MaximumLength(64);
    }
}
