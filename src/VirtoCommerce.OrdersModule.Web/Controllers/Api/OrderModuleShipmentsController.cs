using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Authorization;

namespace VirtoCommerce.OrdersModule.Web.Controllers.Api
{
    [Route("api/order/shipments")]
    [Authorize]
    public class OrderModuleShipmentsController : Controller
    {
        private readonly IShipmentService _shipmentService;
        private readonly IAuthorizationService _authorizationService;

        public OrderModuleShipmentsController(IShipmentService shipmentService,
            IAuthorizationService authorizationService)
        {
            _shipmentService = shipmentService;
            _authorizationService = authorizationService;
        }

        [HttpPost]
        public async Task<ActionResult> UpdateShipment([FromBody] Shipment shipment)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                user: User,
                resource: null,
                requirement: new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.UpdateShipments));
            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }

            await _shipmentService.SaveChangesAsync(new[] { shipment });

            return Ok();
        }
    }
}
