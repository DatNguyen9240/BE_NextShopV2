using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;

namespace NextShopV2.Application.Interfaces.services
{
    public interface IProductCategoryService
    {
        Task<ProductCategoryResponse> AssignProductToCategoryAsync(AssignProductToCategoryRequest request);
        Task<bool> RemoveProductFromCategoryAsync(Guid productId, Guid categoryId);
        Task<List<ProductCategoryResponse>> GetProductCategoriesAsync(Guid productId);
        Task<List<ProductCategoryResponse>> GetCategoryProductsAsync(Guid categoryId);
        Task<List<ProductCategoryResponse>> GetAllProductCategoriesAsync();
        Task<bool> RemoveAllProductCategoriesAsync(Guid productId);
        Task<List<ProductCategoryResponse>> BulkAssignProductToCategoriesAsync(BulkAssignProductToCategoriesRequest request);
        Task<bool> ProductCategoryExistsAsync(Guid productId, Guid categoryId);
    }
}