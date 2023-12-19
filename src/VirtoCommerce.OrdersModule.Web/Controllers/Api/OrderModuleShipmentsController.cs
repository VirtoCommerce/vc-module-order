using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Authorization;

namespace VirtoCommerce.OrdersModule.Web.Controllers.Api
{
    [Route("api/order/shipments")]
    [Authorize]
    public class OrderModuleShipmentsController : Controller
    {
        private readonly IShipmentSearchService _shipmentSearchService;
        private readonly IAuthorizationService _authorizationService;

        public OrderModuleShipmentsController(IShipmentSearchService shipmentSearchService,
            IAuthorizationService authorizationService)
        {
            _shipmentSearchService = shipmentSearchService;
            _authorizationService = authorizationService;
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
    }
}
