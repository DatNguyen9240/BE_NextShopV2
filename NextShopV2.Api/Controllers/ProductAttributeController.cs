using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Api.Attributes;
using System;
using System.Threading.Tasks;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductAttributeController : ControllerBase
    {
        private readonly IProductAttributeService _service;

        public ProductAttributeController(IProductAttributeService service)
        {
            _service = service;
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProductId(Guid productId)
        {
            var attrs = await _service.GetByProductIdAsync(productId);
            return Ok(new { data = attrs });
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategoryId(Guid categoryId)
        {
            var attrs = await _service.GetByCategoryIdAsync(categoryId);
            return Ok(new { data = attrs });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var attrs = await _service.GetAllAsync();
            return Ok(new { data = attrs });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var attr = await _service.GetByIdAsync(id);
            if (attr == null) return NotFound();
            return Ok(new { data = attr });
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> Create([FromBody] NextShopV2.Application.DTOs.Request.CreateAttributeRequest request)
        {
            var r = await _service.CreateAttributeAsync(request);
            return Ok(new { data = r });
        }

        [HttpPost("value")]
        [AdminOnly]
        public async Task<IActionResult> CreateValue([FromBody] NextShopV2.Application.DTOs.Request.CreateAttributeValueRequest request)
        {
            var v = await _service.CreateAttributeValueAsync(request);
            return Ok(new { data = v });
        }

        [HttpGet("{id}/values")]
        public async Task<IActionResult> GetValues(Guid id)
        {
            var v = await _service.GetValuesByAttributeIdAsync(id);
            return Ok(new { data = v });
        }

        [HttpPut("{id}")]
        [AdminOnly]
        public async Task<IActionResult> UpdateAttribute(Guid id, [FromBody] NextShopV2.Application.DTOs.Request.UpdateAttributeRequest request)
        {
            var r = await _service.UpdateAttributeAsync(id, request);
            if (r == null) return NotFound();
            return Ok(new { data = r });
        }

        [HttpDelete("{id}")]
        [AdminOnly]
        public async Task<IActionResult> DeleteAttribute(Guid id)
        {
            var ok = await _service.DeleteAttributeAsync(id);
            if (!ok) return NotFound();
            return Ok(new { success = true });
        }

        [HttpPut("value/{id}")]
        [AdminOnly]
        public async Task<IActionResult> UpdateAttributeValue(Guid id, [FromBody] NextShopV2.Application.DTOs.Request.UpdateAttributeValueRequest request)
        {
            var r = await _service.UpdateAttributeValueAsync(id, request);
            if (r == null) return NotFound();
            return Ok(new { data = r });
        }

        [HttpDelete("value/{id}")]
        [AdminOnly]
        public async Task<IActionResult> DeleteAttributeValue(Guid id)
        {
            var ok = await _service.DeleteAttributeValueAsync(id);
            if (!ok) return NotFound();
            return Ok(new { success = true });
        }
        [HttpPost("{categoryId}/assign/{attributeId}")]
        [AdminOnly]
        public async Task<IActionResult> AssignToCategory(Guid categoryId, Guid attributeId)
        {
            await _service.AssignAttributeToCategoryAsync(categoryId, attributeId);
            return Ok(new { success = true });
        }

        [HttpDelete("{categoryId}/assign/{attributeId}")]
        [AdminOnly]
        public async Task<IActionResult> RemoveFromCategory(Guid categoryId, Guid attributeId)
        {
            await _service.RemoveAttributeFromCategoryAsync(categoryId, attributeId);
            return Ok(new { success = true });
        }

        [HttpGet("{id}/categories")]
        public async Task<IActionResult> GetCategoriesForAttribute(Guid id)
        {
            var ids = await _service.GetCategoriesByAttributeIdAsync(id);
            return Ok(new { data = ids });
        }
    }
}