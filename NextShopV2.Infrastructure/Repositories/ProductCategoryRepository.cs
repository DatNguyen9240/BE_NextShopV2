using Microsoft.EntityFrameworkCore;
using NextShopV2.Application.Interfaces.repositories;
using NextShopV2.Domain.Entities.Products;
using NextShopV2.Infrastructure.Persistence;

namespace NextShopV2.Infrastructure.Repositories
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly AppDbContext _context;

        public ProductCategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProductCategory> AddAsync(ProductCategory productCategory)
        {
            _context.ProductCategories.Add(productCategory);
            await _context.SaveChangesAsync();
            return productCategory;
        }

        public async Task<bool> RemoveAsync(Guid productId, Guid categoryId)
        {
            var productCategory = await _context.ProductCategories
                .FirstOrDefaultAsync(pc => pc.ProductId == productId && pc.CategoryId == categoryId);

            if (productCategory == null)
                return false;

            _context.ProductCategories.Remove(productCategory);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid productId, Guid categoryId)
        {
            return await _context.ProductCategories
                .AnyAsync(pc => pc.ProductId == productId && pc.CategoryId == categoryId);
        }

        public async Task<List<ProductCategory>> GetProductCategoriesAsync(Guid productId)
        {
            return await _context.ProductCategories
                .Where(pc => pc.ProductId == productId)
                .ToListAsync();
        }

        public async Task<List<ProductCategory>> GetCategoryProductsAsync(Guid categoryId)
        {
            return await _context.ProductCategories
                .Where(pc => pc.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<List<ProductCategory>> GetAllAsync()
        {
            return await _context.ProductCategories
                .Include(pc => pc.Product)
                .Include(pc => pc.Category)
                .ToListAsync();
        }

        public async Task<bool> RemoveAllProductCategoriesAsync(Guid productId)
        {
            var productCategories = await _context.ProductCategories
                .Where(pc => pc.ProductId == productId)
                .ToListAsync();

            if (!productCategories.Any())
                return false;

            _context.ProductCategories.RemoveRange(productCategories);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BulkAssignAsync(Guid productId, List<Guid> categoryIds)
        {
            var productCategories = categoryIds.Select(categoryId => new ProductCategory
            {
                ProductId = productId,
                CategoryId = categoryId
            }).ToList();

            _context.ProductCategories.AddRange(productCategories);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<List<ProductCategory>> GetProductCategoriesWithDetailsAsync(Guid productId)
        {
            return await _context.ProductCategories
                .Include(pc => pc.Product)
                .Include(pc => pc.Category)
                .Where(pc => pc.ProductId == productId)
                .ToListAsync();
        }

        public async Task<List<ProductCategory>> GetCategoryProductsWithDetailsAsync(Guid categoryId)
        {
            return await _context.ProductCategories
                .Include(pc => pc.Product)
                .Include(pc => pc.Category)
                .Where(pc => pc.CategoryId == categoryId)
                .ToListAsync();
        }
    }
}