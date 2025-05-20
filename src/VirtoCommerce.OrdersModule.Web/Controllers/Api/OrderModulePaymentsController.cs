using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
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
    public class OrderModulePaymentsController : Controller
    {
        private readonly IPaymentSearchService _paymentSearchService;
        private readonly IPaymentService _paymentService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICustomerOrderService _customerOrderService;
        private readonly IValidator<PaymentIn> _paymentInValidator;
        private readonly ISettingsManager _settingsManager;
        private readonly IPaymentFlowService _paymentFlowService;
        private readonly PaymentDistributedLockOptions _paymentDistributedLockOptions;
        private readonly IDistributedLockService _distributedLockService;

        public OrderModulePaymentsController(
            IPaymentSearchService paymentSearchService,
            IPaymentService paymentService,
            IAuthorizationService authorizationService,
            ICustomerOrderService customerOrderService,
            IValidator<PaymentIn> paymentInValidator,
            ISettingsManager settingsManager,
            IPaymentFlowService paymentFlowService,
            IOptions<PaymentDistributedLockOptions> paymentDistributedLockOptions,
            IDistributedLockService distributedLockService)
        {
            _paymentSearchService = paymentSearchService;
            _paymentService = paymentService;
            _authorizationService = authorizationService;
            _customerOrderService = customerOrderService;
            _paymentInValidator = paymentInValidator;
            _settingsManager = settingsManager;
            _paymentFlowService = paymentFlowService;
            _paymentDistributedLockOptions = paymentDistributedLockOptions.Value;
            _distributedLockService = distributedLockService;
        }

        /// <summary>
        /// Search  order payments by given criteria
        /// </summary>
        /// <param name="criteria">criteria</param>
        [HttpPost]
        [Route("search")]
        public async Task<ActionResult<PaymentSearchResult>> SearchOrderPayments([FromBody] PaymentSearchCriteria criteria)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, criteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var result = await _paymentSearchService.SearchAsync(criteria);

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

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, searchCriteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var result = await _paymentSearchService.SearchAsync(searchCriteria);

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

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, searchCriteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var searchResult = await _paymentSearchService.SearchAsync(searchCriteria);

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
            var customerOrder = await _customerOrderService.GetByIdAsync(payment.OrderId);
            if (customerOrder == null)
            {
                return BadRequest($"order with id: {nameof(payment.OrderId)} is not found");
            }
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, customerOrder, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
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
                    Errors = validationResult.Errors
                });
            }
            await _paymentService.SaveChangesAsync([payment]);
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
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, searchCriteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var result = await _paymentSearchService.SearchAsync(searchCriteria);
            await _paymentService.DeleteAsync(result.Results.Select(x => x.Id).ToArray());
            return Ok();
        }

        [HttpPost]
        [Route("payment/capture")]
        [Authorize(ModuleConstants.Security.Permissions.CapturePayment)]
        public async Task<ActionResult> CapturePayment([FromBody] CaptureOrderPaymentRequest request)
        {
            var resourceKey = $"{nameof(CapturePayment)}:{request.PaymentId ?? request.OrderId}";
            var result = await _distributedLockService.ExecuteAsync(resourceKey,
                () => _paymentFlowService.CapturePaymentAsync(request),
                _paymentDistributedLockOptions.LockTimeout,
                _paymentDistributedLockOptions.TryLockTimeout,
                _paymentDistributedLockOptions.RetryInterval);

            return Ok(result);
        }

        [HttpPost]
        [Route("payment/refund")]
        [Authorize(ModuleConstants.Security.Permissions.RefundPayment)]
        public async Task<ActionResult> RefundPayment([FromBody] RefundOrderPaymentRequest request)
        {
            var resourceKey = $"{nameof(RefundPayment)}:{request.PaymentId ?? request.OrderId}";
            var result = await _distributedLockService.ExecuteAsync(resourceKey,
                () => _paymentFlowService.RefundPaymentAsync(request),
                _paymentDistributedLockOptions.LockTimeout,
                _paymentDistributedLockOptions.TryLockTimeout,
                _paymentDistributedLockOptions.RetryInterval);

            return Ok(result);
        }

        private async Task<ValidationResult> ValidateAsync(PaymentIn paymentIn)
        {
            if (await _settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.CustomerOrderValidation))
            {
                return await _paymentInValidator.ValidateAsync(paymentIn);
            }

            return new ValidationResult();
        }
    }
}
