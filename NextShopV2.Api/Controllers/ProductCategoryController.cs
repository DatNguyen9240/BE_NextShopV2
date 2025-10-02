using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.Interfaces.services;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductCategoryController : ControllerBase
    {
        private readonly IProductCategoryService _productCategoryService;

        public ProductCategoryController(IProductCategoryService productCategoryService)
        {
            _productCategoryService = productCategoryService;
        }

        /// <summary>
        /// Assign a product to a category
        /// </summary>
        [HttpPost("assign")]
        public async Task<IActionResult> AssignProductToCategory([FromBody] AssignProductToCategoryRequest request)
        {
            try
            {
                var result = await _productCategoryService.AssignProductToCategoryAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while assigning product to category", details = ex.Message });
            }
        }

        /// <summary>
        /// Bulk assign a product to multiple categories
        /// </summary>
        [HttpPost("bulk-assign")]
        public async Task<IActionResult> BulkAssignProductToCategories([FromBody] BulkAssignProductToCategoriesRequest request)
        {
            try
            {
                var result = await _productCategoryService.BulkAssignProductToCategoriesAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while bulk assigning product to categories", details = ex.Message });
            }
        }

        /// <summary>
        /// Remove a product from a category
        /// </summary>
        [HttpDelete("remove/{productId:guid}/{categoryId:guid}")]
        public async Task<IActionResult> RemoveProductFromCategory(Guid productId, Guid categoryId)
        {
            try
            {
                var result = await _productCategoryService.RemoveProductFromCategoryAsync(productId, categoryId);
                if (!result)
                {
                    return NotFound(new { message = "Product-Category assignment not found" });
                }

                return Ok(new { message = "Product removed from category successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing product from category", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all categories assigned to a product
        /// </summary>
        [HttpGet("product/{productId:guid}/categories")]
        public async Task<IActionResult> GetProductCategories(Guid productId)
        {
            try
            {
                var result = await _productCategoryService.GetProductCategoriesAsync(productId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving product categories", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all products in a category
        /// </summary>
        [HttpGet("category/{categoryId:guid}/products")]
        public async Task<IActionResult> GetCategoryProducts(Guid categoryId)
        {
            try
            {
                var result = await _productCategoryService.GetCategoryProductsAsync(categoryId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving category products", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all product-category assignments
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllProductCategories()
        {
            try
            {
                var result = await _productCategoryService.GetAllProductCategoriesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving all product categories", details = ex.Message });
            }
        }

        /// <summary>
        /// Remove all category assignments from a product
        /// </summary>
        [HttpDelete("product/{productId:guid}/categories")]
        public async Task<IActionResult> RemoveAllProductCategories(Guid productId)
        {
            try
            {
                var result = await _productCategoryService.RemoveAllProductCategoriesAsync(productId);
                if (!result)
                {
                    return NotFound(new { message = "No categories found for this product" });
                }

                return Ok(new { message = "All categories removed from product successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing all categories from product", details = ex.Message });
            }
        }

        /// <summary>
        /// Check if a product is assigned to a category
        /// </summary>
        [HttpGet("exists/{productId:guid}/{categoryId:guid}")]
        public async Task<IActionResult> ProductCategoryExists(Guid productId, Guid categoryId)
        {
            try
            {
                var exists = await _productCategoryService.ProductCategoryExistsAsync(productId, categoryId);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking product-category assignment", details = ex.Message });
            }
        }
    }
}