using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Domain.Entities.Products;

namespace NextShopV2.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryTransactionRepository _inventoryTransactionRepository;
        private readonly IProductVariantRepository _productVariantRepository;

        public InventoryService(
            IInventoryTransactionRepository inventoryTransactionRepository,
            IProductVariantRepository productVariantRepository)
        {
            _inventoryTransactionRepository = inventoryTransactionRepository;
            _productVariantRepository = productVariantRepository;
        }

        public async Task<bool> CheckStockAvailabilityAsync(Guid variantId, int requiredQty)
        {
            var variant = await _productVariantRepository.GetByIdAsync(variantId);
            if (variant == null) return false;

            return variant.StockQuantity >= requiredQty;
        }

        public async Task<int> GetCurrentStockAsync(Guid variantId)
        {
            var variant = await _productVariantRepository.GetByIdAsync(variantId);
            return variant?.StockQuantity ?? 0;
        }

        public async Task<IEnumerable<InventoryTransaction>> GetInventoryHistoryAsync(Guid variantId)
        {
            return await _inventoryTransactionRepository.GetByVariantIdAsync(variantId);
        }

        public async Task<bool> UpdateInventoryAsync(Guid variantId, int changeQty, string reason, string createdBy)
        {
            try
            {
                // Get current variant
                var variant = await _productVariantRepository.GetByIdAsync(variantId);
                if (variant == null) return false;

                // Check if reducing stock would result in negative quantity
                if (changeQty < 0 && variant.StockQuantity + changeQty < 0)
                {
                    return false; // Not enough stock
                }

                // Create inventory transaction
                var transaction = new InventoryTransaction
                {
                    TransactionId = Guid.NewGuid(),
                    VariantId = variantId,
                    ChangeQty = changeQty,
                    Reason = reason,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                };

                await _inventoryTransactionRepository.CreateAsync(transaction);

                // Update variant stock quantity
                variant.StockQuantity += changeQty;
                await _productVariantRepository.UpdateAsync(variant);
                // Persist variant changes
                await _productVariantRepository.SaveAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}