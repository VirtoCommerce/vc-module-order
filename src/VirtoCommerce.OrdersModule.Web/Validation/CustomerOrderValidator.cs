using FluentValidation;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Web.Validation
{
    public class CustomerOrderValidator : AbstractValidator<CustomerOrder>
    {
        public CustomerOrderValidator()
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

            RuleFor(order => order.DiscountAmount).ScalePrecision(4, 19);
            RuleFor(order => order.TaxTotal).ScalePrecision(4, 19);
            RuleFor(order => order.Total).ScalePrecision(4, 19);
            RuleFor(order => order.SubTotal).ScalePrecision(4, 19);
            RuleFor(order => order.SubTotalWithTax).ScalePrecision(4, 19);
            RuleFor(order => order.ShippingTotal).ScalePrecision(4, 19);
            RuleFor(order => order.ShippingTotalWithTax).ScalePrecision(4, 19);
            RuleFor(order => order.PaymentTotal).ScalePrecision(4, 19);
            RuleFor(order => order.PaymentTotalWithTax).ScalePrecision(4, 19);
            RuleFor(order => order.DiscountTotal).ScalePrecision(4, 19);
            RuleFor(order => order.DiscountTotalWithTax).ScalePrecision(4, 19);
        }
    }
}
