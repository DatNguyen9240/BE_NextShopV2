using NextShopV2.Domain.Entities.Products;

namespace NextShopV2.Application.Interfaces.repositories
{
    public interface ICategoryRepository
    {
        Task<Category> AddAsync(Category category);
        Task<Category?> GetByIdAsync(Guid id);
        Task<List<Category>> GetAllAsync();
        Task<List<Category>> GetRootCategoriesAsync();
        Task<List<Category>> GetChildCategoriesAsync(Guid parentId);
        Task<Category> UpdateAsync(Category category);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> NameExistsAtLevelAsync(string name, Guid? parentId, Guid? excludeId = null);
        Task<bool> HasChildrenAsync(Guid id);
        Task<bool> HasProductsAsync(Guid id);
        Task<string?> GetParentNameAsync(Guid parentId);
        Task<bool> IsCircularReferenceAsync(Guid categoryId, Guid newParentId);
    }
}