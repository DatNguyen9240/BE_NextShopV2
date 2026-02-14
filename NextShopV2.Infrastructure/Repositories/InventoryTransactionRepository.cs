using Microsoft.EntityFrameworkCore;
using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Domain.Entities.Products;
using NextShopV2.Infrastructure.Persistence;

namespace NextShopV2.Infrastructure.Repositories
{
    public class InventoryTransactionRepository : IInventoryTransactionRepository
    {
        private readonly AppDbContext _context;

        public InventoryTransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<InventoryTransaction> CreateAsync(InventoryTransaction transaction)
        {
            _context.InventoryTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<IEnumerable<InventoryTransaction>> GetAllAsync()
        {
            return await _context.InventoryTransactions
                .Include(it => it.Variant)
                .OrderByDescending(it => it.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.InventoryTransactions
                .Include(it => it.Variant)
                .Where(it => it.CreatedAt >= startDate && it.CreatedAt <= endDate)
                .OrderByDescending(it => it.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryTransaction>> GetByVariantIdAsync(Guid variantId)
        {
            return await _context.InventoryTransactions
                .Include(it => it.Variant)
                .Where(it => it.VariantId == variantId)
                .OrderByDescending(it => it.CreatedAt)
                .ToListAsync();
        }

        // Dashboard metrics
        public async Task<int> GetCountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.InventoryTransactions
                .Where(it => it.CreatedAt >= startDate && it.CreatedAt <= endDate)
                .CountAsync();
        }

        public async Task<List<InventoryTransaction>> GetRecentAsync(int limit)
        {
            return await _context.InventoryTransactions
                .Include(it => it.Variant)
                .OrderByDescending(it => it.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }
    }
}