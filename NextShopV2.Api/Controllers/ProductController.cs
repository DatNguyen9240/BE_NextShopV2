using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Api.Helpers;
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
        public ProductController(IProductService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var products = await _service.GetAllAsync();
                return ResponseHelper.Success(products, "Products retrieved successfully");
            }
            catch (Exception ex)
            {
                return ResponseHelper.Error(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var product = await _service.GetByIdAsync(id);
                if (product == null)
                    return ResponseHelper.NotFound("Product not found");
                
                return ResponseHelper.Success(product, "Product retrieved successfully");
            }
            catch (Exception ex)
            {
                return ResponseHelper.Error(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return ResponseHelper.ValidationError(ModelState);

                var created = await _service.CreateAsync(request);
                return ResponseHelper.Created(created, "Product created successfully");
            }
            catch (ArgumentException ex)
            {
                return ResponseHelper.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return ResponseHelper.Error(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return ResponseHelper.ValidationError(ModelState);

                var result = await _service.UpdateAsync(id, request);
                if (!result)
                    return ResponseHelper.NotFound("Product not found");

                return ResponseHelper.Success(message: "Product updated successfully");
            }
            catch (ArgumentException ex)
            {
                return ResponseHelper.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return ResponseHelper.Error(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok)
                return NotFound(new ApiResponse { Success = false, Message = "Product not found" });
            return Ok(new ApiResponse { Success = true });
        }
    }
}
