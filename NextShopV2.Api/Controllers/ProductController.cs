using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Shared.Helpers;
using NextShopV2.Shared.Extensions;
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
        public async Task<IActionResult> GetAll(string? section, string? categoryId = null, int page = 1, int pageSize = 12)
        {
            _logger.LogInformation("ProductController.GetAll called with section={Section}, categoryId={CategoryId}, page={Page}, pageSize={PageSize}", section ?? "<null>", categoryId ?? "<null>", page, pageSize);

            if (string.IsNullOrEmpty(section) && string.IsNullOrEmpty(categoryId))
            {
                var products = await _service.GetAllAsync();
                return ResponseHelper.Success(products, "Products retrieved successfully");
            }

            Guid? catGuid = null;
            if (!string.IsNullOrEmpty(categoryId) && Guid.TryParse(categoryId, out var parsed)) catGuid = parsed;

            var paged = await _service.GetBySectionAsync(section, catGuid, page, pageSize);
            return ResponseHelper.Success(paged, "Products retrieved successfully");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _service.GetByIdAsync(id);
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
