using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Domain.Entities.Products;
using NextShopV2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;
        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<Product>> GetAllAsync()
            => await _context.Products
                .Include(p => p.Variants.OrderBy(v => v.DisplayOrder))
                .ToListAsync();
        public async Task<Product?> GetByIdAsync(Guid id)
            => await _context.Products
                .Include(p => p.Variants.OrderBy(v => v.DisplayOrder))
                .FirstOrDefaultAsync(p => p.ProductId == id);
        public async Task<bool> ExistsAsync(Guid id)
            => await _context.Products.AnyAsync(p => p.ProductId == id);
        public async Task AddAsync(Product product)
            => await _context.Products.AddAsync(product);
        public Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            return Task.CompletedTask;
        }
        public Task DeleteAsync(Product product)
        {
            _context.Products.Remove(product);
            return Task.CompletedTask;
        }
        public async Task SaveAsync()
            => await _context.SaveChangesAsync();
    }
}
