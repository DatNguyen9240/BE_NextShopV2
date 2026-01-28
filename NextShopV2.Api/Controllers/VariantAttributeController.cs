using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Request;
using System;
using System.Threading.Tasks;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VariantAttributeController : ControllerBase
    {
        private readonly IProductAttributeService _service;

        public VariantAttributeController(IProductAttributeService service)
        {
            _service = service;
        }

        [HttpGet("variant/{variantId}")]
        public async Task<IActionResult> GetByVariant(Guid variantId)
        {
            var ids = await _service.GetVariantAttributeValueIdsAsync(variantId);
            return Ok(new { data = ids });
        }

        [HttpPost]
        public async Task<IActionResult> Assign([FromBody] AssignVariantAttributeRequest request)
        {
            if (request == null || request.VariantId == Guid.Empty || request.AttributeValueId == Guid.Empty)
            {
                return BadRequest(new { error = "VariantId and AttributeValueId are required" });
            }

            try
            {
                await _service.AssignVariantAttributeValueAsync(request);
                return Ok(new { success = true });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                // Unexpected
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Remove([FromQuery] Guid variantId, [FromQuery] Guid attributeValueId)
        {
            await _service.RemoveVariantAttributeValueAsync(variantId, attributeValueId);
            return Ok(new { success = true });
        }
    }
}