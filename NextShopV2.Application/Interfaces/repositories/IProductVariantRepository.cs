using NextShopV2.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Application.Interfaces.Repositories
{
    public interface IProductVariantRepository
    {
        Task<List<ProductVariant>> GetAllAsync();
        Task<ProductVariant?> GetByIdAsync(Guid id);
        Task<List<ProductVariant>> GetByProductIdAsync(Guid productId);
        Task<List<ProductVariant>> GetByIdsAsync(List<Guid> ids);
        Task<ProductVariant?> GetDefaultByProductIdAsync(Guid productId);
        Task<bool> ExistsAsync(Guid id);
        Task AddAsync(ProductVariant variant);
        Task UpdateAsync(ProductVariant variant);
        Task DeleteAsync(Guid id);
        Task SaveAsync();

        // Raw lookup for debugging active flags
        Task<List<(Guid VariantId, bool IsActive)>> GetActiveFlagsByProductIdAsync(Guid productId);
    }
}