using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Shared.Helpers;
using NextShopV2.Shared.Extensions;
using NextShopV2.Shared.Extensions.Web;
using NextShopV2.Api.Attributes;
using System;
using System.Threading.Tasks;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductVariantController : ControllerBase
    {
        private readonly IProductVariantService _variantService;
        private readonly NextShopV2.Application.Interfaces.Repositories.IProductVariantRepository _variantRepo;

        public ProductVariantController(IProductVariantService variantService, NextShopV2.Application.Interfaces.Repositories.IProductVariantRepository variantRepo)
        {
            _variantService = variantService;
            _variantRepo = variantRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Public store view: always exclude inactive variants. Use admin endpoints to fetch all.
            var variants = await _variantService.GetAllAsync() ?? new List<ProductVariantResponse>();
            variants = variants.Where(v => v != null && v.IsActive.GetValueOrDefault(false)).ToList();
            return ResponseHelper.Success(variants, "Variants retrieved successfully");
        }

        // Admin-only endpoints that return all variants (including inactive)
        [AdminOnly]
        [HttpGet("admin")]
        public async Task<IActionResult> GetAllAdmin()
        {
            var variants = await _variantService.GetAllAsync() ?? new List<ProductVariantResponse>();
            return ResponseHelper.Success(variants, "Variants retrieved successfully");
        }

        [AdminOnly]
        [HttpGet("admin/{id}")]
        public async Task<IActionResult> GetByIdAdmin(Guid id)
        {
            var variant = await _variantService.GetByIdAsync(id);
            if (variant == null)
                return ResponseHelper.NotFound("Variant not found");

            return ResponseHelper.Success(variant, "Variant retrieved successfully");
        }

        [AdminOnly]
        [HttpGet("admin/product/{productId}")]
        public async Task<IActionResult> GetByProductIdAdmin(Guid productId)
        {
            var variants = await _variantService.GetByProductIdAsync(productId) ?? new List<ProductVariantResponse>();
            return ResponseHelper.Success(variants, "Product variants retrieved successfully");
        }

        [AdminOnly]
        [HttpGet("admin/product/{productId}/default")]
        public async Task<IActionResult> GetDefaultByProductIdAdmin(Guid productId)
        {
            var variant = await _variantService.GetDefaultByProductIdAsync(productId);
            if (variant == null)
                return ResponseHelper.NotFound("No default variant found for this product");

            return ResponseHelper.Success(variant, "Default variant retrieved successfully");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // Public store view: exclude inactive variants.
            var variant = await _variantService.GetByIdAsync(id);
            if (variant == null || variant.IsActive != true)
                return ResponseHelper.NotFound("Variant not found");

            return ResponseHelper.Success(variant, "Variant retrieved successfully");
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProductId(Guid productId)
        {
            // Public store view: exclude inactive variants.
            var variants = await _variantService.GetByProductIdAsync(productId) ?? new List<ProductVariantResponse>();
            variants = variants.Where(v => v != null && v.IsActive.GetValueOrDefault(false)).ToList();

            return ResponseHelper.Success(variants, "Product variants retrieved successfully");
        }

        [HttpGet("product/{productId}/default")]
        public async Task<IActionResult> GetDefaultByProductId(Guid productId)
        {
            // Public store view: only return default if it's active.
            var variant = await _variantService.GetDefaultByProductIdAsync(productId);
            if (variant == null || variant.IsActive != true)
                return ResponseHelper.NotFound("No default variant found for this product");

            return ResponseHelper.Success(variant, "Default variant retrieved successfully");
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> Create([FromBody] CreateProductVariantRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.ValidationError(ModelState);

            var variant = await _variantService.CreateAsync(request);
            return ResponseHelper.Created(variant, "Variant created successfully");
        }

        [HttpPut("{id}")]
        [AdminOnly]
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
        [AdminOnly]
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
        [AdminOnly]
        public async Task<IActionResult> SetAsDefault(Guid id)
        {
            var success = await _variantService.SetAsDefaultAsync(id);
            if (!success)
                return ResponseHelper.NotFound("Variant not found");

            return ResponseHelper.Success(null, "Variant set as default successfully");
        }

        [HttpDelete("{id}")]
        [AdminOnly]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _variantService.DeleteAsync(id);
            if (!success)
                return ResponseHelper.NotFound("Variant not found");

            return ResponseHelper.Success(null, "Variant deleted successfully");
        }
    }
}