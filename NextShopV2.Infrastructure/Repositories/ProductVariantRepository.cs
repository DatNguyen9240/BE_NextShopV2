using Microsoft.EntityFrameworkCore;
using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Domain.Entities.Products;
using NextShopV2.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextShopV2.Infrastructure.Repositories
{
    public class ProductVariantRepository : IProductVariantRepository
    {
        private readonly AppDbContext _context;

        public ProductVariantRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductVariant>> GetAllAsync()
        {
            return await _context.ProductVariants
                .Include(v => v.Product)
                .OrderBy(v => v.DisplayOrder)
                .ToListAsync();
        }

        public async Task<ProductVariant?> GetByIdAsync(Guid id)
        {
            return await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.VariantId == id);
        }

        public async Task<List<ProductVariant>> GetByProductIdAsync(Guid productId)
        {
            var list = await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .OrderBy(v => v.DisplayOrder)
                .ToListAsync();

            return list;
        }

        public async Task<List<(Guid VariantId, bool IsActive)>> GetActiveFlagsByProductIdAsync(Guid productId)
        {
            return await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .Select(v => new ValueTuple<Guid, bool>(v.VariantId, v.IsActive))
                .ToListAsync();
        }

        public async Task<ProductVariant?> GetDefaultByProductIdAsync(Guid productId)
        {
            // First try to find explicitly marked default variant
            var defaultVariant = await _context.ProductVariants
                .Where(v => v.ProductId == productId && v.IsDefault)
                .FirstOrDefaultAsync();

            if (defaultVariant != null) return defaultVariant;

            // Fall back to first variant ordered by DisplayOrder
            return await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .OrderBy(v => v.DisplayOrder)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.ProductVariants.AnyAsync(v => v.VariantId == id);
        }

        public async Task AddAsync(ProductVariant variant)
        {
            await _context.ProductVariants.AddAsync(variant);
        }

        public Task UpdateAsync(ProductVariant variant)
        {
            _context.ProductVariants.Update(variant);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var variant = await GetByIdAsync(id);
            if (variant != null)
            {
                _context.ProductVariants.Remove(variant);
            }
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}