using NextShopV2.Domain.Entities.Products;

namespace NextShopV2.Application.Interfaces.Services
{
    public interface IInventoryService
    {
        Task<bool> UpdateInventoryAsync(Guid variantId, int changeQty, string reason, string createdBy);
        Task<IEnumerable<InventoryTransaction>> GetInventoryHistoryAsync(Guid variantId);
        Task<int> GetCurrentStockAsync(Guid variantId);
        Task<bool> CheckStockAvailabilityAsync(Guid variantId, int requiredQty);
    }
}