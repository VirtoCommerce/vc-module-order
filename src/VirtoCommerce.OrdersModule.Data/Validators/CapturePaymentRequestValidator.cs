using System;
using System.Linq;
using FluentValidation;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Data.Validators
{
    public class CapturePaymentRequestValidator : AbstractValidator<OrderPaymentInfo>
    {
        private readonly PaymentStatus[] _validStates = new[] { PaymentStatus.Authorized };

        public CapturePaymentRequestValidator()
        {
            RuleFor(x => x.CustomerOrder)
                .NotNull()
                .WithMessage(PaymentErrorDescriber.OrderNotFound());

            RuleFor(x => x.Payment)
                .NotNull()
                .WithMessage(PaymentErrorDescriber.PaymentNotFound());

            RuleFor(x => x.Payment.PaymentMethod)
                .NotNull()
                .WithMessage(x => PaymentErrorDescriber.PaymentMethodNotFound(x.Payment.GatewayCode))
                .When(x => x.Payment != null);

            RuleFor(x => x.Store)
                .NotNull()
                .WithMessage(PaymentErrorDescriber.StoreNotFound());

            RuleFor(x => x)
                .Custom((request, context) =>
                {
                    if (!_validStates.Contains(request.Payment.PaymentStatus))
                    {
                        context.AddFailure(PaymentErrorDescriber.InvalidStatus(request.Payment.PaymentStatus));
                    }
                })
                .When(x => x.Payment != null);
        }
    }
}
