using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.Interfaces.services;
using NextShopV2.Api.Helpers;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/Category
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return ResponseHelper.Success(categories, "Categories retrieved successfully");
            }
            catch (Exception ex)
            {
                return ResponseHelper.Error(ex.Message);
            }
        }

        // GET: api/Category/root
        [HttpGet("root")]
        public async Task<IActionResult> GetRootCategories()
        {
            try
            {
                var categories = await _categoryService.GetRootCategoriesAsync();
                return ResponseHelper.Success(categories, "Root categories retrieved successfully");
            }
            catch (Exception ex)
            {
                return ResponseHelper.Error(ex.Message);
            }
        }

        // GET: api/Category/{id}/children
        [HttpGet("{id}/children")]
        public async Task<IActionResult> GetChildCategories(Guid id)
        {
            try
            {
                var categories = await _categoryService.GetChildCategoriesAsync(id);
                return ResponseHelper.Success(categories, "Child categories retrieved successfully");
            }
            catch (Exception ex)
            {
                return ResponseHelper.Error(ex.Message);
            }
        }

        // GET: api/Category/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                    return ResponseHelper.NotFound("Category not found");
                
                return ResponseHelper.Success(category, "Category retrieved successfully");
            }
            catch (Exception ex)
            {
                return ResponseHelper.Error(ex.Message);
            }
        }

        // POST: api/Category
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return ResponseHelper.ValidationError(ModelState);

                var category = await _categoryService.CreateCategoryAsync(request);
                return ResponseHelper.Created(category, "Category created successfully");
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

        // PUT: api/Category/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateCategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return ResponseHelper.ValidationError(ModelState);

                var category = await _categoryService.UpdateCategoryAsync(id, request);
                return ResponseHelper.Success(category, "Category updated successfully");
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

        // DELETE: api/Category/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);
                if (!result)
                    return ResponseHelper.NotFound("Category not found");

                return ResponseHelper.Success(message: "Category deleted successfully");
            }
            catch (InvalidOperationException ex)
            {
                return ResponseHelper.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return ResponseHelper.Error(ex.Message);
            }
        }
    }
}