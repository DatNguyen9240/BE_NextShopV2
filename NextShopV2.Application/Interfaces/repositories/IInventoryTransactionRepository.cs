using NextShopV2.Domain.Entities.Products;

namespace NextShopV2.Application.Interfaces.Repositories
{
    public interface IInventoryTransactionRepository
    {
        Task<InventoryTransaction> CreateAsync(InventoryTransaction transaction);
        Task<IEnumerable<InventoryTransaction>> GetByVariantIdAsync(Guid variantId);
        Task<IEnumerable<InventoryTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<InventoryTransaction>> GetAllAsync();

        // Dashboard metrics
        Task<int> GetCountByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<InventoryTransaction>> GetRecentAsync(int limit);
    }
}