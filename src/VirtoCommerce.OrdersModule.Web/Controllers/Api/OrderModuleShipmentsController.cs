using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Authorization;

namespace VirtoCommerce.OrdersModule.Web.Controllers.Api
{
    [Route("api/order/shipments")]
    [Authorize]
    public class OrderModuleShipmentsController(IShipmentService shipmentService,
        IShipmentSearchService shipmentSearchService,
        IAuthorizationService authorizationService)
        : Controller
    {
        [HttpPost]
        [Authorize(ModuleConstants.Security.Permissions.UpdateShipments)]
        public async Task<ActionResult> UpdateShipment([FromBody] Shipment shipment)
        {
            await shipmentService.SaveChangesAsync([shipment]);

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
            var authorizationResult = await authorizationService.AuthorizeAsync(User, criteria, new OrderAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var result = await shipmentSearchService.SearchAsync(criteria);

            return Ok(result);
        }

        /// <summary>
        /// Partial update for the specified Shipment by id
        /// </summary>
        /// <param name="id">Shipment id</param>
        /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}")]
        [Authorize(ModuleConstants.Security.Permissions.UpdateShipments)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> PatchShipment(string id, [FromBody] JsonPatchDocument<Shipment> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var shipment = (await shipmentService.GetAsync([id])).FirstOrDefault();
            if (shipment == null)
            {
                return NotFound();
            }

            patchDocument.ApplyTo(shipment, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await shipmentService.SaveChangesAsync([shipment]);

            return NoContent();
        }
    }
}
