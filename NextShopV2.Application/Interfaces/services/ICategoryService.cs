using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;

namespace NextShopV2.Application.Interfaces.services
{
    public interface ICategoryService
    {
        Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request);
        Task<CategoryResponse?> GetCategoryByIdAsync(Guid id);
        Task<List<CategoryResponse>> GetAllCategoriesAsync();
        Task<CategoryResponse> UpdateCategoryAsync(Guid id, CreateCategoryRequest request);
        Task<bool> DeleteCategoryAsync(Guid id);
        Task<List<CategoryResponse>> GetRootCategoriesAsync();
        Task<List<CategoryResponse>> GetChildCategoriesAsync(Guid parentId);
        Task<List<CategoryResponse>> GetCategoryTreeAsync();
    }
}