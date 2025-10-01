using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Api.Helpers;
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
            try
            {
                var variants = await _variantService.GetAllAsync();
                var response = ResponseHelper.Success(variants, "Variants retrieved successfully");
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseHelper.Error(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var variant = await _variantService.GetByIdAsync(id);
                if (variant == null)
                    return NotFound(ResponseHelper.Error("Variant not found"));

                var response = ResponseHelper.Success(variant, "Variant retrieved successfully");
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseHelper.Error(ex.Message));
            }
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProductId(Guid productId)
        {
            try
            {
                var variants = await _variantService.GetByProductIdAsync(productId);
                var response = ResponseHelper.Success(variants, "Product variants retrieved successfully");
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseHelper.Error(ex.Message));
            }
        }

        [HttpGet("product/{productId}/default")]
        public async Task<IActionResult> GetDefaultByProductId(Guid productId)
        {
            try
            {
                var variant = await _variantService.GetDefaultByProductIdAsync(productId);
                if (variant == null)
                    return NotFound(ResponseHelper.Error("No default variant found for this product"));

                var response = ResponseHelper.Success(variant, "Default variant retrieved successfully");
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseHelper.Error(ex.Message));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductVariantRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ResponseHelper.Error("Invalid data"));

                var variant = await _variantService.CreateAsync(request);
                var response = ResponseHelper.Success(variant, "Variant created successfully");
                return new JsonResult(response) { StatusCode = 201 };
            }
            catch (ArgumentException ex)
            {
                return NotFound(ResponseHelper.Error(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseHelper.Error(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductVariantRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ResponseHelper.Error("Invalid data"));

                var success = await _variantService.UpdateAsync(id, request);
                if (!success)
                    return NotFound(ResponseHelper.Error("Variant not found"));

                var response = ResponseHelper.Success(null, "Variant updated successfully");
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseHelper.Error(ex.Message));
            }
        }

        [HttpPatch("{id}/stock")]
        public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ResponseHelper.Error("Invalid data"));

                var success = await _variantService.UpdateStockAsync(id, request);
                if (!success)
                    return NotFound(ResponseHelper.Error("Variant not found"));

                var response = ResponseHelper.Success(null, "Stock updated successfully");
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseHelper.Error(ex.Message));
            }
        }

        [HttpPatch("{id}/set-default")]
        public async Task<IActionResult> SetAsDefault(Guid id)
        {
            try
            {
                var success = await _variantService.SetAsDefaultAsync(id);
                if (!success)
                    return NotFound(ResponseHelper.Error("Variant not found"));

                var response = ResponseHelper.Success(null, "Variant set as default successfully");
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseHelper.Error(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _variantService.DeleteAsync(id);
                if (!success)
                    return NotFound(ResponseHelper.Error("Variant not found"));

                var response = ResponseHelper.Success(null, "Variant deleted successfully");
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseHelper.Error(ex.Message));
            }
        }
    }
}