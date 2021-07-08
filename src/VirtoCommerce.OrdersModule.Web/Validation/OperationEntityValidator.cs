using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using VirtoCommerce.OrdersModule.Data.Model;

namespace VirtoCommerce.OrdersModule.Web.Validation
{
    public class OperationEntityValidator : AbstractValidator<OperationEntity>
    {
        public OperationEntityValidator()
        {
            RuleFor(operation => operation.Number).NotEmpty().NotNull().MaximumLength(64);
            RuleFor(operation => operation.Status).MaximumLength(64);
            RuleFor(operation => operation.Comment).MaximumLength(2048);
            RuleFor(operation => operation.Currency).NotNull().NotEmpty().MaximumLength(3);
            RuleFor(operation => operation.Sum).ScalePrecision(4, 19);
            RuleFor(operation => operation.CancelReason).MaximumLength(2048);
            RuleFor(operation => operation.OuterId).MaximumLength(128);
        }
    }
}
