using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.OrdersModule.Web.Validation;

public class CustomerOrderValidator : AbstractValidator<CustomerOrder>
{
    private readonly ISettingsManager _settingsManager;

    public CustomerOrderValidator(
        ISettingsManager settingsManager,
        IEnumerable<IValidator<LineItem>> lineItemValidators,
        IEnumerable<IValidator<Shipment>> shipmentValidators,
        IValidator<PaymentIn> paymentInValidator,
        IEnumerable<IValidator<IOperation>> operationValidators)
    {
        _settingsManager = settingsManager;

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

        // Apply all operation-level validators (e.g., document count limits)
        foreach (var operationValidator in operationValidators)
        {
            Include(operationValidator);
        }
    }

    public override async Task<ValidationResult> ValidateAsync(ValidationContext<CustomerOrder> context, CancellationToken cancellation = default)
    {
        // Check if validation is enabled
        var isValidationEnabled = await _settingsManager.GetValueAsync<bool>(
            ModuleConstants.Settings.General.CustomerOrderValidation);

        if (!isValidationEnabled)
        {
            // Skip validation if disabled
            return new ValidationResult();
        }

        // Perform validation if enabled
        return await base.ValidateAsync(context, cancellation);
    }
    protected void SetDefaultRules()
    {
#pragma warning disable S109
        RuleFor(order => order.Number).NotEmpty().MaximumLength(64);
        RuleFor(order => order.CustomerId).NotNull().NotEmpty().MaximumLength(64);
        RuleFor(order => order.CustomerName).NotEmpty().MaximumLength(255);
        RuleFor(order => order.StoreId).NotNull().NotEmpty().MaximumLength(64);
        RuleFor(order => order.StoreName).MaximumLength(255);
        RuleFor(order => order.ChannelId).MaximumLength(64);
        RuleFor(order => order.OrganizationId).MaximumLength(64);
        RuleFor(order => order.OrganizationName).MaximumLength(255);
        RuleFor(order => order.EmployeeId).MaximumLength(64);
        RuleFor(order => order.EmployeeName).MaximumLength(255);
        RuleFor(order => order.SubscriptionId).MaximumLength(64);
        RuleFor(order => order.SubscriptionNumber).MaximumLength(64);
        RuleFor(order => order.LanguageCode).MaximumLength(16)
            .Matches("^[a-z]{2}-[A-Z]{2}$")
            .When(order => !string.IsNullOrEmpty(order.LanguageCode));
        RuleFor(order => order.ShoppingCartId).MaximumLength(128);
        RuleFor(order => order.PurchaseOrderNumber).MaximumLength(128);
#pragma warning restore S109
    }
}
