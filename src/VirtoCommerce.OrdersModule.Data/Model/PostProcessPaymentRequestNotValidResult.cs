using System.Collections.Generic;
using FluentValidation.Results;
using VirtoCommerce.PaymentModule.Model.Requests;

namespace VirtoCommerce.OrdersModule.Data.Model;

public class PostProcessPaymentRequestNotValidResult : PostProcessPaymentRequestResult
{
    public IList<ValidationFailure> Errors { get; set; }
}
