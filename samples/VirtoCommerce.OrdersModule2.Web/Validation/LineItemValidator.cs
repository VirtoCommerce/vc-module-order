using FluentValidation;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule2.Web.Validation
{
    public class LineItemValidator : AbstractValidator<LineItem>
    {
        public LineItemValidator()
        {
            RuleFor(item => item.Name).NotEmpty().NotNull().MaximumLength(128);
            RuleFor(item => item.Quantity).LessThan(100).NotNull();
        }
    }
}
