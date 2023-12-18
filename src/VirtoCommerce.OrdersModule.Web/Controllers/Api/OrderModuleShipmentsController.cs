using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Authorization;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.OrdersModule.Web.Controllers.Api
{
    [Route("api/order/shipments")]
    [Authorize]
    public class OrderModuleShipmentsController : Controller
    {
        private readonly IShipmentService _shipmentService;
        private readonly IShipmentSearchService _shipmentSearchService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICustomerOrderService _customerOrderService;
        private readonly ISettingsManager _settingsManager;
        private readonly IValidator<Shipment> _shipmentValidator;

        public OrderModuleShipmentsController(IShipmentService shipmentService,
            IShipmentSearchService shipmentSearchService,
            IAuthorizationService authorizationService,
            ICustomerOrderService customerOrderService,
            ISettingsManager settingsManager,
            IEnumerable<IValidator<Shipment>> shipmentValidators)
        {
            _shipmentService = shipmentService;
            _shipmentSearchService = shipmentSearchService;
            _authorizationService = authorizationService;
            _settingsManager = settingsManager;
            _customerOrderService = customerOrderService;

            _shipmentValidator = null;
            if (shipmentValidators.Any())
            {
                _shipmentValidator = shipmentValidators.Last();
            }
        }

        /// <summary>
        /// Create or update order shipment
        /// </summary>
        /// <param name="shipment">shipment</param>
        [HttpPost]
        [Route("")]
        public async Task<ActionResult<CustomerOrder>> CreateShipment([FromBody] Shipment shipment)
        {
            if (shipment == null)
            {
                return BadRequest($"{nameof(shipment)} is required");
            }
            if (string.IsNullOrEmpty(shipment.CustomerOrderId))
            {
                return BadRequest($"{nameof(shipment.CustomerOrderId)} is required");
            }
            var customerOrder = await _customerOrderService.GetByIdAsync(shipment.CustomerOrderId);
            if (customerOrder == null)
            {
                return BadRequest($"order with id: {nameof(shipment.CustomerOrderId)} is not found");
            }
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, customerOrder, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var validationResult = await ValidateAsync(shipment);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    Message = string.Join(" ", validationResult.Errors.Select(x => x.ErrorMessage)),
                    Errors = validationResult.Errors
                });
            }
            await _shipmentService.SaveChangesAsync(new[] { shipment });
            return Ok(shipment);
        }

        [HttpPost]
        [Authorize(ModuleConstants.Security.Permissions.UpdateShipments)]
        public async Task<ActionResult> UpdateShipment([FromBody] Shipment shipment)
        {
            await _shipmentService.SaveChangesAsync(new[] { shipment });

            return Ok();
        }

        /// <summary>
        /// Search shipments by given criteria
        /// </summary>
        /// <param name="criteria">criteria</param>
        [HttpPost]
        [Route("search")]
        public async Task<ActionResult<ShipmentSearchResult>> SearchOrderShipments([FromBody] ShipmentSearchCriteria criteria)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, criteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var result = await _shipmentSearchService.SearchAsync(criteria);

            return Ok(result);
        }

        /// <summary>
        /// Find order shipment by id
        /// </summary>
        /// <remarks>Return a single order shipment with all nested documents or null if shipment was not found</remarks>
        /// <param name="id">order shipment id</param>
        /// <param name="respGroup"></param>
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Shipment>> GetById(string id, [SwaggerOptional][FromQuery] string respGroup = null)
        {
            var searchCriteria = AbstractTypeFactory<ShipmentSearchCriteria>.TryCreateInstance();
            searchCriteria.Ids = new[] { id };
            searchCriteria.ResponseGroup = respGroup;
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, searchCriteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var result = await _shipmentSearchService.SearchAsync(searchCriteria);

            return Ok(result.Results.FirstOrDefault());
        }

        /// <summary>
        ///  Delete a shipment
        /// </summary>
        /// <param name="ids">shipment ids</param>
        [HttpDelete]
        [Route("")]
        public async Task<ActionResult> DeleteOrderShipmentsByIds([FromQuery] string[] ids)
        {
            if (ids == null)
            {
                return BadRequest($"{nameof(ids)} is required");
            }
            var searchCriteria = AbstractTypeFactory<ShipmentSearchCriteria>.TryCreateInstance();
            searchCriteria.Ids = ids;
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, searchCriteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var result = await _shipmentSearchService.SearchAsync(searchCriteria);
            await _shipmentService.DeleteAsync(result.Results.Select(x => x.Id).ToArray());
            return Ok();
        }

        private async Task<ValidationResult> ValidateAsync(Shipment shipment)
        {
            if (_shipmentValidator != null &&
                await _settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.CustomerOrderValidation))
            {
                return await _shipmentValidator.ValidateAsync(shipment);
            }

            return new ValidationResult();
        }
    }
}
