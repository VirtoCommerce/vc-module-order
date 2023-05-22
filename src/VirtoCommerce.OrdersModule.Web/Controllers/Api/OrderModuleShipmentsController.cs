using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;

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
        [Authorize(ModuleConstants.Security.Permissions.UpdateShipments)]
        public async Task<ActionResult> UpdateShipment([FromBody] Shipment shipment)
        {
            var source = new CancellationTokenSource();
            var token = source.Token;

            await _shipmentService.SaveChangesAsync(new[] { shipment }, token);

            return Ok();
        }
    }
}
