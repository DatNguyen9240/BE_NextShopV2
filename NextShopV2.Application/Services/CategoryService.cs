using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Application.Interfaces.services;
using NextShopV2.Application.Interfaces.repositories;
using NextShopV2.Domain.Entities.Products;
using NextShopV2.Shared.Extensions;

namespace NextShopV2.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
        {
            // Validate parent category exists if ParentId is provided
            if (request.ParentId.HasValue)
            {
                var parentExists = await _categoryRepository.ExistsAsync(request.ParentId.Value);
                if (!parentExists)
                {
                    throw new ArgumentException("Parent category not found");
                }
            }

            // Check if category name already exists at the same level
            var nameExists = await _categoryRepository.NameExistsAtLevelAsync(request.Name, request.ParentId);
            if (nameExists)
            {
                throw new ArgumentException("Category with this name already exists at this level");
            }

            // Create new category
            var category = new Category
            {
                CategoryId = Guid.NewGuid(),
                Name = request.Name,
                ParentId = request.ParentId,
                CreatedAt = DateTime.UtcNow,
                ImageUrl = request.ImageUrl,
                Icon = request.Icon
            };

            var createdCategory = await _categoryRepository.AddAsync(category);

            // Return response
            return new CategoryResponse
            {
                CategoryId = createdCategory.CategoryId,
                Name = createdCategory.Name,
                ParentId = createdCategory.ParentId,
                CreatedAt = createdCategory.CreatedAt,
                ImageUrl = createdCategory.ImageUrl,
                Icon = createdCategory.Icon,
                ParentName = request.ParentId.HasValue 
                    ? await _categoryRepository.GetParentNameAsync(request.ParentId.Value) 
                    : null
            };
        }

        public async Task<CategoryResponse?> GetCategoryByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category is not null)
            {
                return MapToResponse(category);
            }

            return null;
        }

        public async Task<List<CategoryResponse>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();

            return categories.Select(MapToResponse).ToList();
        }

        public async Task<CategoryResponse> UpdateCategoryAsync(Guid id, CreateCategoryRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category is null)
            {
                throw new ArgumentException("Category not found");
            }

            // Validate parent category if changing
            if (request.ParentId.HasValue && request.ParentId != category.ParentId)
            {
                var parentExists = await _categoryRepository.ExistsAsync(request.ParentId.Value);
                if (!parentExists)
                {
                    throw new ArgumentException("Parent category not found");
                }

                // Prevent circular reference
                if (await _categoryRepository.IsCircularReferenceAsync(id, request.ParentId.Value))
                {
                    throw new ArgumentException("Cannot set parent to a child category");
                }
            }

            // Check name uniqueness (excluding current category)
            var nameExists = await _categoryRepository.NameExistsAtLevelAsync(request.Name, request.ParentId, id);
            if (nameExists)
            {
                throw new ArgumentException("Category with this name already exists at this level");
            }

            category.Name = request.Name;
            category.ParentId = request.ParentId;

            var updatedCategory = await _categoryRepository.UpdateAsync(category);

            return new CategoryResponse
            {
                CategoryId = updatedCategory.CategoryId,
                Name = updatedCategory.Name,
                ParentId = updatedCategory.ParentId,
                CreatedAt = updatedCategory.CreatedAt,
                ParentName = request.ParentId.HasValue 
                    ? await _categoryRepository.GetParentNameAsync(request.ParentId.Value) 
                    : null
            };
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var categoryExists = await _categoryRepository.ExistsAsync(id);
            if (!categoryExists) return false;

            // Check if category has children
            if (await _categoryRepository.HasChildrenAsync(id))
            {
                throw new InvalidOperationException("Cannot delete category that has child categories");
            }

            // Check if category has products
            if (await _categoryRepository.HasProductsAsync(id))
            {
                throw new InvalidOperationException("Cannot delete category that has products");
            }

            return await _categoryRepository.DeleteAsync(id);
        }

        public async Task<List<CategoryResponse>> GetRootCategoriesAsync()
        {
            var categories = await _categoryRepository.GetRootCategoriesAsync();
            return categories.Select(MapToResponse).ToList();
        }

        public async Task<List<CategoryResponse>> GetChildCategoriesAsync(Guid parentId)
        {
            var categories = await _categoryRepository.GetChildCategoriesAsync(parentId);
            return categories.Select(MapToResponse).ToList();
        }

        public async Task<List<CategoryResponse>> GetCategoryTreeAsync()
        {
            // Build a tree from all categories to ensure nested children are assembled correctly
            var allCategories = await _categoryRepository.GetAllAsync();

            // Reset children collections and rebuild a parent -> children relationship
            var lookup = allCategories.ToDictionary(c => c.CategoryId);
            foreach (var cat in lookup.Values)
            {
                cat.Children = new List<Category>();
            }

            foreach (var cat in allCategories)
            {
                if (cat.ParentId.HasValue && lookup.ContainsKey(cat.ParentId.Value))
                {
                    lookup[cat.ParentId.Value].Children.Add(cat);
                }
            }

            var roots = lookup.Values.Where(c => c.ParentId == null).OrderBy(c => c.Name).ToList();

            return roots.Select(MapToResponse).ToList();
        }



        private CategoryResponse MapToResponse(Category category)
        {
            return new CategoryResponse
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                ParentId = category.ParentId,
                CreatedAt = category.CreatedAt,
                ParentName = category.Parent?.Name,
                Children = category.Children?.Select(MapToResponse).ToList() ?? new List<CategoryResponse>()
            };
        }
    }
}