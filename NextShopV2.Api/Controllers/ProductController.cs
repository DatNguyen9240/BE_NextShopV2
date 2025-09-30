using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Response;
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
            var products = await _service.GetAllAsync();
            return Ok(new ApiResponse { Success = true, Data = products });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _service.GetByIdAsync(id);
            if (product == null)
                return NotFound(new ApiResponse { Success = false, Message = "Product not found" });
            return Ok(new ApiResponse { Success = true, Data = product });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return Ok(new ApiResponse { Success = true, Data = created });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ProductDto dto)
        {
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok)
                return NotFound(new ApiResponse { Success = false, Message = "Product not found" });
            return Ok(new ApiResponse { Success = true });
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
