using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using VirtoCommerce.OrdersModule.Data.Model;

namespace VirtoCommerce.OrdersModule.Data.Validation
{
    public class ShipmentEntityValidator: AbstractValidator<ShipmentEntity>
    {
        public ShipmentEntityValidator()
        {
            RuleFor(shipment => shipment.OrganizationId).MaximumLength(64);
            RuleFor(shipment => shipment.OrganizationName).MaximumLength(255);

            RuleFor(shipment => shipment.FulfillmentCenterId).MaximumLength(64);
            RuleFor(shipment => shipment.FulfillmentCenterName).MaximumLength(255);

            RuleFor(shipment => shipment.EmployeeId).MaximumLength(64);
            RuleFor(shipment => shipment.EmployeeName).MaximumLength(255);

            RuleFor(shipment => shipment.ShipmentMethodCode).MaximumLength(64);
            RuleFor(shipment => shipment.ShipmentMethodOption).MaximumLength(64);

            RuleFor(shipment => shipment.WeightUnit).MaximumLength(32);
            RuleFor(shipment => shipment.MeasureUnit).MaximumLength(32);

            RuleFor(shipment => shipment.TaxType).MaximumLength(64);

            RuleFor(shipment => shipment.Price).ScalePrecision(4, 19);
            RuleFor(shipment => shipment.PriceWithTax).ScalePrecision(4, 19);

            RuleFor(shipment => shipment.DiscountAmount).ScalePrecision(4, 19);
            RuleFor(shipment => shipment.DiscountAmountWithTax).ScalePrecision(4, 19);

            RuleFor(shipment => shipment.Total).ScalePrecision(4, 19);
            RuleFor(shipment => shipment.TotalWithTax).ScalePrecision(4, 19);

            RuleFor(shipment => shipment.TaxTotal).ScalePrecision(4, 19);
            RuleFor(shipment => shipment.TaxPercentRate).ScalePrecision(4, 19);
        }
    }
}
