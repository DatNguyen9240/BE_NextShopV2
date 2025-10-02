using NextShopV2.Domain.Entities.Products;

namespace NextShopV2.Application.Interfaces.repositories
{
    public interface IProductCategoryRepository
    {
        Task<ProductCategory> AddAsync(ProductCategory productCategory);
        Task<bool> RemoveAsync(Guid productId, Guid categoryId);
        Task<bool> ExistsAsync(Guid productId, Guid categoryId);
        Task<List<ProductCategory>> GetProductCategoriesAsync(Guid productId);
        Task<List<ProductCategory>> GetCategoryProductsAsync(Guid categoryId);
        Task<List<ProductCategory>> GetAllAsync();
        Task<bool> RemoveAllProductCategoriesAsync(Guid productId);
        Task<bool> BulkAssignAsync(Guid productId, List<Guid> categoryIds);
        Task<List<ProductCategory>> GetProductCategoriesWithDetailsAsync(Guid productId);
        Task<List<ProductCategory>> GetCategoryProductsWithDetailsAsync(Guid categoryId);
    }
}