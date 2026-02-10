using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Shared.Helpers;
using NextShopV2.Shared.Extensions;
using NextShopV2.Shared.Extensions.Web;
using NextShopV2.Api.Attributes;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;
        private readonly Microsoft.Extensions.Logging.ILogger<ProductController> _logger;
        public ProductController(IProductService service, Microsoft.Extensions.Logging.ILogger<ProductController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string? categoryId = null, int page = 1, int pageSize = 12, decimal? minPrice = null, decimal? maxPrice = null, string? sort = null, string? search = null)
        {
            _logger.LogInformation("ProductController.GetAll called with categoryId={CategoryId}, page={Page}, pageSize={PageSize}, minPrice={MinPrice}, maxPrice={MaxPrice}, sort={Sort}, search={Search}", categoryId ?? "<null>", page, pageSize, minPrice?.ToString() ?? "<null>", maxPrice?.ToString() ?? "<null>", sort ?? "<null>", search ?? "<null>");

            // Public store view: always exclude inactive items. Admins can use the admin endpoints instead.

            // If there are absolutely no filters or pagination provided, return the full list.
            // Otherwise use the paged/filtering endpoint which supports price/sort/paging/search.
            if (string.IsNullOrEmpty(categoryId) && !minPrice.HasValue && !maxPrice.HasValue && string.IsNullOrEmpty(sort) && string.IsNullOrEmpty(search) && page == 1 && pageSize == 12)
            {
                var products = await _service.GetAllAsync(includeInactive: false);
                return ResponseHelper.Success(products, "Products retrieved successfully");
            }

            Guid? catGuid = null;
            if (!string.IsNullOrEmpty(categoryId) && Guid.TryParse(categoryId, out var parsed)) catGuid = parsed;

            var paged = await _service.GetPagedAsync(catGuid, page, pageSize, minPrice, maxPrice, sort, search, includeInactive: false);
            return ResponseHelper.Success(paged, "Products retrieved successfully");
        }

        // Admin-only endpoints to retrieve items including inactive
        [AdminOnly]
        [HttpGet("admin")]
        public async Task<IActionResult> GetAllAdmin(string? categoryId = null, int page = 1, int pageSize = 12, decimal? minPrice = null, decimal? maxPrice = null, string? sort = null, string? search = null)
        {
            // If no filters/paging requested, return full list including inactive
            if (string.IsNullOrEmpty(categoryId) && !minPrice.HasValue && !maxPrice.HasValue && string.IsNullOrEmpty(sort) && string.IsNullOrEmpty(search) && page == 1 && pageSize == 12)
            {
                var products = await _service.GetAllAsync(includeInactive: true);
                return ResponseHelper.Success(products, "Products retrieved successfully");
            }

            Guid? catGuid = null;
            if (!string.IsNullOrEmpty(categoryId) && Guid.TryParse(categoryId, out var parsed)) catGuid = parsed;

            var paged = await _service.GetPagedAsync(catGuid, page, pageSize, minPrice, maxPrice, sort, search, includeInactive: true);
            return ResponseHelper.Success(paged, "Products retrieved successfully");
        }

        [AdminOnly]
        [HttpGet("admin/{id}")]
        public async Task<IActionResult> GetByIdAdmin(Guid id)
        {
            var product = await _service.GetByIdAsync(id, includeInactive: true);
            if (product.IsNull())
                return ResponseHelper.NotFound("Product not found");

            return ResponseHelper.Success(product, "Product retrieved successfully");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // Public view: exclude inactive products. Admins can call the admin endpoint if needed.
            var product = await _service.GetByIdAsync(id, includeInactive: false);
            if (product.IsNull())
                return ResponseHelper.NotFound("Product not found");
            
            return ResponseHelper.Success(product, "Product retrieved successfully");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.ValidationError(ModelState);

            var created = await _service.CreateAsync(request);
            return ResponseHelper.Created(created, "Product created successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.ValidationError(ModelState);

            var result = await _service.UpdateAsync(id, request);
            if (!result)
                return ResponseHelper.NotFound("Product not found");

            return ResponseHelper.Success(message: "Product updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
                return ResponseHelper.NotFound("Product not found");
            
            return ResponseHelper.Success(message: "Product deleted successfully");
        }
    }
}
