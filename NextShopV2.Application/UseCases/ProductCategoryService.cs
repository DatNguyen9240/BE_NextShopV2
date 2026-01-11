using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Application.Interfaces.repositories;
using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Application.Interfaces.services;
using NextShopV2.Domain.Entities.Products;

namespace NextShopV2.Application.Services
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductCategoryService(
            IProductCategoryRepository productCategoryRepository,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository)
        {
            _productCategoryRepository = productCategoryRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<ProductCategoryResponse> AssignProductToCategoryAsync(AssignProductToCategoryRequest request)
        {
            // Validate product exists
            var productExists = await _productRepository.ExistsAsync(request.ProductId);
            if (!productExists)
            {
                throw new ArgumentException("Product not found");
            }

            // Validate category exists
            var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);
            if (!categoryExists)
            {
                throw new ArgumentException("Category not found");
            }

            // Check if assignment already exists
            var assignmentExists = await _productCategoryRepository.ExistsAsync(request.ProductId, request.CategoryId);
            if (assignmentExists)
            {
                throw new ArgumentException("Product is already assigned to this category");
            }

            // Create assignment
            var productCategory = new ProductCategory
            {
                ProductId = request.ProductId,
                CategoryId = request.CategoryId,
                AssignedAt = DateTime.UtcNow
            };

            var createdAssignment = await _productCategoryRepository.AddAsync(productCategory);

            // Get product and category details for response
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId);

            return new ProductCategoryResponse
            {
                ProductId = createdAssignment.ProductId,
                CategoryId = createdAssignment.CategoryId,
                ProductName = product?.Name ?? string.Empty,
                CategoryName = category?.Name ?? string.Empty,
                AssignedAt = createdAssignment.AssignedAt
            };
        }

        public async Task<bool> RemoveProductFromCategoryAsync(Guid productId, Guid categoryId)
        {
            var exists = await _productCategoryRepository.ExistsAsync(productId, categoryId);
            if (!exists)
            {
                return false;
            }

            return await _productCategoryRepository.RemoveAsync(productId, categoryId);
        }

        public async Task<List<ProductCategoryResponse>> GetProductCategoriesAsync(Guid productId)
        {
            var productCategories = await _productCategoryRepository.GetProductCategoriesWithDetailsAsync(productId);
            return productCategories.Select(MapToResponse).ToList();
        }

        public async Task<List<ProductCategoryResponse>> GetCategoryProductsAsync(Guid categoryId)
        {
            var categoryProducts = await _productCategoryRepository.GetCategoryProductsWithDetailsAsync(categoryId);
            return categoryProducts.Select(MapToResponse).ToList();
        }

        public async Task<List<ProductCategoryResponse>> GetAllProductCategoriesAsync()
        {
            var productCategories = await _productCategoryRepository.GetAllAsync();
            return productCategories.Select(MapToResponse).ToList();
        }

        public async Task<bool> RemoveAllProductCategoriesAsync(Guid productId)
        {
            return await _productCategoryRepository.RemoveAllProductCategoriesAsync(productId);
        }

        public async Task<List<ProductCategoryResponse>> BulkAssignProductToCategoriesAsync(BulkAssignProductToCategoriesRequest request)
        {
            // Validate product exists
            var productExists = await _productRepository.ExistsAsync(request.ProductId);
            if (!productExists)
            {
                throw new ArgumentException("Product not found");
            }

            // Validate all categories exist
            foreach (var categoryId in request.CategoryIds)
            {
                var categoryExists = await _categoryRepository.ExistsAsync(categoryId);
                if (!categoryExists)
                {
                    throw new ArgumentException($"Category with ID {categoryId} not found");
                }
            }

            // Remove existing assignments for this product
            await _productCategoryRepository.RemoveAllProductCategoriesAsync(request.ProductId);

            // Bulk assign new categories
            var success = await _productCategoryRepository.BulkAssignAsync(request.ProductId, request.CategoryIds);
            
            if (!success)
            {
                throw new InvalidOperationException("Failed to assign product to categories");
            }

            // Return the new assignments
            return await GetProductCategoriesAsync(request.ProductId);
        }

        public async Task<bool> ProductCategoryExistsAsync(Guid productId, Guid categoryId)
        {
            return await _productCategoryRepository.ExistsAsync(productId, categoryId);
        }

        private ProductCategoryResponse MapToResponse(ProductCategory productCategory)
        {
            return new ProductCategoryResponse
            {
                ProductId = productCategory.ProductId,
                CategoryId = productCategory.CategoryId,
                ProductName = productCategory.Product?.Name ?? string.Empty,
                CategoryName = productCategory.Category?.Name ?? string.Empty,
                AssignedAt = productCategory.AssignedAt
            };
        }
    }
}