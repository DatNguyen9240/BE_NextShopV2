using Microsoft.EntityFrameworkCore;
using NextShopV2.Application.Interfaces.repositories;
using NextShopV2.Domain.Entities.Products;
using NextShopV2.Infrastructure.Persistence;

namespace NextShopV2.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Category> AddAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category?> GetByIdAsync(Guid id)
        {
            return await _context.Categories
                .Include(c => c.Parent)
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Include(c => c.Parent)
                .Include(c => c.Children)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<Category>> GetRootCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.Children)
                .Where(c => c.ParentId == null)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<Category>> GetChildCategoriesAsync(Guid parentId)
        {
            return await _context.Categories
                .Include(c => c.Children)
                .Where(c => c.ParentId == parentId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category> UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Categories.AnyAsync(c => c.CategoryId == id);
        }

        public async Task<bool> NameExistsAtLevelAsync(string name, Guid? parentId, Guid? excludeId = null)
        {
            var query = _context.Categories
                .Where(c => c.Name.ToLower() == name.ToLower() && c.ParentId == parentId);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.CategoryId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> HasChildrenAsync(Guid id)
        {
            return await _context.Categories.AnyAsync(c => c.ParentId == id);
        }

        public async Task<bool> HasProductsAsync(Guid id)
        {
            return await _context.ProductCategories.AnyAsync(pc => pc.CategoryId == id);
        }

        public async Task<string?> GetParentNameAsync(Guid parentId)
        {
            var parent = await _context.Categories.FindAsync(parentId);
            return parent?.Name;
        }

        public async Task<bool> IsCircularReferenceAsync(Guid categoryId, Guid newParentId)
        {
            Guid? currentParentId = newParentId;
            
            while (currentParentId.HasValue)
            {
                if (currentParentId.Value == categoryId)
                    return true;

                var parent = await _context.Categories.FindAsync(currentParentId.Value);
                currentParentId = parent?.ParentId;
            }

            return false;
        }
    }
}