using System;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Data.Validators
{
    public class PaymentRequestValidator : AbstractValidator<OrderPaymentInfo>
    {
        public const string DefaultRuleSet = "default";
        public const string CaptureRuleSet = "capture";
        public const string RefundRuleSet = "refund";

        private readonly PaymentStatus[] _captureValidStates = new[] { PaymentStatus.Authorized };
        private readonly PaymentStatus[] _refundValidStates = new[] { PaymentStatus.Paid, PaymentStatus.PartiallyRefunded };

        public PaymentRequestValidator()
        {
            RuleFor(x => x.CustomerOrder)
                .NotNull()
                .WithErrorCode(PaymentFlowErrorCodes.InvalidRequestError)
                .WithMessage(PaymentErrorDescriber.OrderNotFound());

            RuleFor(x => x.Payment)
                .NotNull()
                .WithErrorCode(PaymentFlowErrorCodes.InvalidRequestError)
                .WithMessage(PaymentErrorDescriber.PaymentNotFound());

            RuleFor(x => x.Payment.PaymentMethod)
                .NotNull()
                .WithErrorCode(PaymentFlowErrorCodes.PaymentProviderUnavailable)
                .WithMessage(x => PaymentErrorDescriber.PaymentMethodNotFound(x.Payment.GatewayCode))
                .When(x => x.Payment != null);

            RuleFor(x => x.Store)
                .NotNull()
                .WithErrorCode(PaymentFlowErrorCodes.InvalidRequestError)
                .WithMessage(PaymentErrorDescriber.StoreNotFound());

            RuleSet(CaptureRuleSet, () =>
                RuleFor(x => x)
                    .Custom((request, context) =>
                    {
                        if (request.Payment.PaymentMethod is not ISupportCaptureFlow)
                        {
                            var failure = new ValidationFailure(nameof(request.Payment.PaymentMethod), PaymentErrorDescriber.NotCapturable())
                            {
                                ErrorCode = PaymentFlowErrorCodes.InvalidRequestError
                            };
                            context.AddFailure(failure);
                        }

                        if (!_captureValidStates.Contains(request.Payment.PaymentStatus))
                        {
                            var failure = new ValidationFailure(nameof(request.Payment.PaymentStatus), PaymentErrorDescriber.InvalidStatus(request.Payment.PaymentStatus))
                            {
                                ErrorCode = PaymentFlowErrorCodes.InvalidRequestError
                            };
                            context.AddFailure(failure);
                        }
                    })
                    .When(x => x.Payment != null));

            RuleSet(RefundRuleSet, () =>
                RuleFor(x => x)
                    .Custom((request, context) =>
                    {
                        if (request.Payment.PaymentMethod is not ISupportRefundFlow)
                        {
                            var failure = new ValidationFailure(nameof(request.Payment.PaymentMethod), PaymentErrorDescriber.NotRefundable())
                            {
                                ErrorCode = PaymentFlowErrorCodes.InvalidRequestError
                            };
                            context.AddFailure(failure);
                        }

                        if (!_refundValidStates.Contains(request.Payment.PaymentStatus))
                        {
                            var failure = new ValidationFailure(nameof(request.Payment.PaymentStatus), PaymentErrorDescriber.InvalidStatus(request.Payment.PaymentStatus))
                            {
                                ErrorCode = PaymentFlowErrorCodes.InvalidRequestError
                            };
                            context.AddFailure(failure);
                        }
                    })
                    .When(x => x.Payment != null));
        }
    }
}
