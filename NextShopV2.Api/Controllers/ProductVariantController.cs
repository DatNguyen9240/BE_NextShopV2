using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Shared.Helpers;
using NextShopV2.Shared.Extensions;
using System;
using System.Threading.Tasks;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductVariantController : ControllerBase
    {
        private readonly IProductVariantService _variantService;

        public ProductVariantController(IProductVariantService variantService)
        {
            _variantService = variantService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var variants = await _variantService.GetAllAsync();
            return ResponseHelper.Success(variants, "Variants retrieved successfully");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var variant = await _variantService.GetByIdAsync(id);
            if (variant.IsNull())
                return ResponseHelper.NotFound("Variant not found");

            return ResponseHelper.Success(variant, "Variant retrieved successfully");
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProductId(Guid productId)
        {
            var variants = await _variantService.GetByProductIdAsync(productId);
            return ResponseHelper.Success(variants, "Product variants retrieved successfully");
        }

        [HttpGet("product/{productId}/default")]
        public async Task<IActionResult> GetDefaultByProductId(Guid productId)
        {
            var variant = await _variantService.GetDefaultByProductIdAsync(productId);
            if (variant.IsNull())
                return ResponseHelper.NotFound("No default variant found for this product");

            return ResponseHelper.Success(variant, "Default variant retrieved successfully");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductVariantRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.ValidationError(ModelState);

            var variant = await _variantService.CreateAsync(request);
            return ResponseHelper.Created(variant, "Variant created successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductVariantRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.ValidationError(ModelState);

            var success = await _variantService.UpdateAsync(id, request);
            if (!success)
                return ResponseHelper.NotFound("Variant not found");

            return ResponseHelper.Success(null, "Variant updated successfully");
        }

        [HttpPatch("{id}/stock")]
        public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.ValidationError(ModelState);

            var success = await _variantService.UpdateStockAsync(id, request);
            if (!success)
                return ResponseHelper.NotFound("Variant not found");

            return ResponseHelper.Success(null, "Stock updated successfully");
        }

        [HttpPatch("{id}/set-default")]
        public async Task<IActionResult> SetAsDefault(Guid id)
        {
            var success = await _variantService.SetAsDefaultAsync(id);
            if (!success)
                return ResponseHelper.NotFound("Variant not found");

            return ResponseHelper.Success(null, "Variant set as default successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _variantService.DeleteAsync(id);
            if (!success)
                return ResponseHelper.NotFound("Variant not found");

            return ResponseHelper.Success(null, "Variant deleted successfully");
        }
    }
}