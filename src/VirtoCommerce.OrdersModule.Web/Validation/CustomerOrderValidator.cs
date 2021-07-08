using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Web.Validation
{
    public class CustomerOrderValidator : AbstractValidator<CustomerOrder>
    {
        public CustomerOrderValidator(IEnumerable<IValidator<LineItem>> lineItemValidators,
            IEnumerable<IValidator<Shipment>> shipmentValidators,
            IValidator<PaymentIn> paymentInValidator)
        {
            SetDefaultRules();

            if (lineItemValidators.Any())
            {
                RuleForEach(order => order.Items).SetValidator(lineItemValidators.Last(), "default");
            }

            if (shipmentValidators.Any())
            {
                RuleForEach(order => order.Shipments).SetValidator(shipmentValidators.Last(), "default");
            }

            RuleForEach(order => order.InPayments).SetValidator(paymentInValidator);
        }

        protected void SetDefaultRules()
        {
            RuleFor(order => order.Number).MaximumLength(128);
            RuleFor(order => order.CustomerId).NotNull().NotEmpty().MaximumLength(64);
            RuleFor(order => order.CustomerName).MaximumLength(255);
            RuleFor(order => order.StoreId).NotNull().NotEmpty().MaximumLength(64);
            RuleFor(order => order.StoreName).MaximumLength(255);
            RuleFor(order => order.ChannelId).MaximumLength(64);
            RuleFor(order => order.OrganizationId).MaximumLength(64);
            RuleFor(order => order.OrganizationName).MaximumLength(255);
            RuleFor(order => order.EmployeeId).MaximumLength(64);
            RuleFor(order => order.EmployeeName).MaximumLength(255);
            RuleFor(order => order.SubscriptionId).MaximumLength(64);
            RuleFor(order => order.SubscriptionNumber).MaximumLength(64);
            RuleFor(order => order.LanguageCode).MaximumLength(16);
            RuleFor(order => order.ShoppingCartId).MaximumLength(128);
            RuleFor(order => order.PurchaseOrderNumber).MaximumLength(128);
        }
    }
}
