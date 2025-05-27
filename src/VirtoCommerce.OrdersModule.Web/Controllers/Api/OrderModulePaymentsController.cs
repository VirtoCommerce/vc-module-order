using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Authorization;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DistributedLock;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.OrdersModule.Web.Controllers.Api
{
    [Route("api/order/payments")]
    [Authorize]
    public class OrderModulePaymentsController(
        IPaymentSearchService paymentSearchService,
        IPaymentService paymentService,
        IAuthorizationService authorizationService,
        ICustomerOrderService customerOrderService,
        IValidator<PaymentIn> paymentInValidator,
        ISettingsManager settingsManager,
        IPaymentFlowService paymentFlowService,
        IOptions<PaymentDistributedLockOptions> paymentDistributedLockOptions,
        IDistributedLockService distributedLockService)
        : Controller
    {
        /// <summary>
        /// Search  order payments by given criteria
        /// </summary>
        /// <param name="criteria">criteria</param>
        [HttpPost]
        [Route("search")]
        public async Task<ActionResult<PaymentSearchResult>> SearchOrderPayments([FromBody] PaymentSearchCriteria criteria)
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(User, criteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var result = await paymentSearchService.SearchAsync(criteria);

            return Ok(result);
        }

        /// <summary>
        /// Find  order payment by id
        /// </summary>
        /// <remarks>Return a single order payment with all nested documents or null if payment was not found</remarks>
        /// <param name="id"> order payment id</param>
        /// <param name="respGroup"></param>
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<PaymentIn>> GetById(string id, [SwaggerOptional][FromQuery] string respGroup = null)
        {
            var searchCriteria = AbstractTypeFactory<PaymentSearchCriteria>.TryCreateInstance();
            searchCriteria.Ids = [id];
            searchCriteria.ResponseGroup = respGroup;

            var authorizationResult = await authorizationService.AuthorizeAsync(User, searchCriteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var result = await paymentSearchService.SearchAsync(searchCriteria);

            return Ok(result.Results.FirstOrDefault());
        }

        /// <summary>
        /// Get order payment by outer id
        /// </summary>
        /// <remarks>Return a single order payment with all nested documents or null if payment was not found</remarks>
        /// <param name="outerId"> order payment outer id</param>
        /// <param name="responseGroup"></param>
        [HttpGet]
        [Route("outer/{outerId}")]
        public async Task<ActionResult<PaymentIn>> GetByOuterId(string outerId, [SwaggerOptional][FromQuery] string responseGroup = null)
        {
            var searchCriteria = AbstractTypeFactory<PaymentSearchCriteria>.TryCreateInstance();
            searchCriteria.OuterIds = [outerId];
            searchCriteria.ResponseGroup = responseGroup;

            var authorizationResult = await authorizationService.AuthorizeAsync(User, searchCriteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var searchResult = await paymentSearchService.SearchAsync(searchCriteria);

            return Ok(searchResult.Results.FirstOrDefault());
        }

        /// <summary>
        /// Create or update order payment
        /// </summary>
        /// <param name="payment">payment</param>
        [HttpPost]
        [Route("")]
        public async Task<ActionResult<CustomerOrder>> CreatePayment([FromBody] PaymentIn payment)
        {
            if (payment == null)
            {
                return BadRequest($"{nameof(payment)} is required");
            }
            if (string.IsNullOrEmpty(payment.OrderId))
            {
                return BadRequest($"{nameof(payment.OrderId)} is required");
            }
            var customerOrder = await customerOrderService.GetByIdAsync(payment.OrderId);
            if (customerOrder == null)
            {
                return BadRequest($"order with id: {nameof(payment.OrderId)} is not found");
            }
            var authorizationResult = await authorizationService.AuthorizeAsync(User, customerOrder, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var validationResult = await ValidateAsync(payment);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    Message = string.Join(" ", validationResult.Errors.Select(x => x.ErrorMessage)),
                    validationResult.Errors
                });
            }
            await paymentService.SaveChangesAsync([payment]);
            return Ok(payment);
        }

        [HttpPut]
        [Route("")]
        public Task<ActionResult<CustomerOrder>> UpdatePayment([FromBody] PaymentIn payment)
        {
            return CreatePayment(payment);
        }

        /// <summary>
        ///  Delete an order payment
        /// </summary>
        /// <param name="ids">order payment ids</param>
        [HttpDelete]
        [Route("")]
        public async Task<ActionResult> DeleteOrderPaymentsByIds([FromQuery] string[] ids)
        {
            if (ids == null)
            {
                return BadRequest($"{nameof(ids)} is required");
            }
            var searchCriteria = AbstractTypeFactory<PaymentSearchCriteria>.TryCreateInstance();
            searchCriteria.Ids = ids;
            var authorizationResult = await authorizationService.AuthorizeAsync(User, searchCriteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var result = await paymentSearchService.SearchAsync(searchCriteria);
            await paymentService.DeleteAsync(result.Results.Select(x => x.Id).ToArray());
            return Ok();
        }

        [HttpPost]
        [Route("payment/capture")]
        [Authorize(ModuleConstants.Security.Permissions.CapturePayment)]
        public async Task<ActionResult> CapturePayment([FromBody] CaptureOrderPaymentRequest request)
        {
            var resourceKey = $"{nameof(CapturePayment)}:{request.PaymentId ?? request.OrderId}";
            var result = await distributedLockService.ExecuteAsync(resourceKey,
                () => paymentFlowService.CapturePaymentAsync(request),
                paymentDistributedLockOptions.Value.LockTimeout,
                paymentDistributedLockOptions.Value.TryLockTimeout,
                paymentDistributedLockOptions.Value.RetryInterval);

            return Ok(result);
        }

        [HttpPost]
        [Route("payment/refund")]
        [Authorize(ModuleConstants.Security.Permissions.RefundPayment)]
        public async Task<ActionResult> RefundPayment([FromBody] RefundOrderPaymentRequest request)
        {
            var resourceKey = $"{nameof(RefundPayment)}:{request.PaymentId ?? request.OrderId}";
            var result = await distributedLockService.ExecuteAsync(resourceKey,
                () => paymentFlowService.RefundPaymentAsync(request),
                paymentDistributedLockOptions.Value.LockTimeout,
                paymentDistributedLockOptions.Value.TryLockTimeout,
                paymentDistributedLockOptions.Value.RetryInterval);

            return Ok(result);
        }

        /// <summary>
        /// Partial update for the specified Payment by id
        /// </summary>
        /// <param name="id">Payment id</param>
        /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> PatchPayment(string id, [FromBody] JsonPatchDocument<PaymentIn> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var payment = await paymentService.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            var customerOrder = await customerOrderService.GetByIdAsync(payment.OrderId);
            var authorizationResult = await authorizationService.AuthorizeAsync(User, customerOrder, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            patchDocument.ApplyTo(payment, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var validationResult = await ValidateAsync(payment);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    Message = string.Join(" ", validationResult.Errors.Select(x => x.ErrorMessage)),
                    validationResult.Errors
                });
            }
            await paymentService.SaveChangesAsync([payment]);

            return NoContent();
        }

        private async Task<ValidationResult> ValidateAsync(PaymentIn paymentIn)
        {
            if (await settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.CustomerOrderValidation))
            {
                return await paymentInValidator.ValidateAsync(paymentIn);
            }

            return new ValidationResult();
        }
    }
}
