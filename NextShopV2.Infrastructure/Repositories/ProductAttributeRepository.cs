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
    public class ProductAttributeRepository : IProductAttributeRepository
    {
        private readonly AppDbContext _context;

        public ProductAttributeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductAttribute>> GetAllAsync()
        {
            return await _context.Set<ProductAttribute>()
                .Include(a => a.Values)
                .ToListAsync();
        }

        public async Task<ProductAttribute?> GetByIdAsync(Guid id)
        {
            return await _context.Set<ProductAttribute>()
                .Include(a => a.Values)
                .FirstOrDefaultAsync(a => a.AttributeId == id);
        }

        public async Task<List<ProductAttribute>> GetByCategoryIdAsync(Guid categoryId)
        {
            var attributeIds = await _context.Set<Domain.Entities.Products.CategoryAttribute>()
                .Where(ca => ca.CategoryId == categoryId)
                .Select(ca => ca.AttributeId)
                .ToListAsync();

            return await _context.Set<ProductAttribute>()
                .Where(a => attributeIds.Contains(a.AttributeId))
                .Include(a => a.Values)
                .ToListAsync();
        }

        public async Task<List<ProductAttribute>> GetByProductIdAsync(Guid productId)
        {
            // Determine product categories first
            var categoryIds = await _context.Set<Domain.Entities.Products.ProductCategory>()
                .Where(pc => pc.ProductId == productId)
                .Select(pc => pc.CategoryId)
                .ToListAsync();

            if (!categoryIds.Any()) return new List<ProductAttribute>();

            var attributeIds = await _context.Set<Domain.Entities.Products.CategoryAttribute>()
                .Where(ca => categoryIds.Contains(ca.CategoryId))
                .Select(ca => ca.AttributeId)
                .ToListAsync();

            return await _context.Set<ProductAttribute>()
                .Where(a => attributeIds.Contains(a.AttributeId))
                .Include(a => a.Values)
                .ToListAsync();
        }

        public async Task<List<AttributeValue>> GetValuesByAttributeIdAsync(Guid attributeId)
        {
            return await _context.Set<AttributeValue>()
                .Where(v => v.AttributeId == attributeId)
                .OrderBy(v => v.DisplayOrder)
                .ToListAsync();
        }

        public async Task<AttributeValue?> GetValueByIdAsync(Guid id)
        {
            return await _context.Set<AttributeValue>()
                .FirstOrDefaultAsync(v => v.AttributeValueId == id);
        }

        public async Task AddAttributeValueAsync(AttributeValue value)
        {
            await _context.Set<AttributeValue>().AddAsync(value);
        }

        public Task UpdateAttributeValueAsync(AttributeValue value)
        {
            _context.Set<AttributeValue>().Update(value);
            return Task.CompletedTask;
        }

        public async Task DeleteAttributeValueAsync(Guid id)
        {
            var v = await GetValueByIdAsync(id);
            if (v != null) _context.Set<AttributeValue>().Remove(v);
        }


        public async Task<List<Domain.Entities.Products.VariantAttributeValue>> GetVariantAttributeValuesAsync(Guid variantId)
        {
            return await _context.Set<Domain.Entities.Products.VariantAttributeValue>()
                .Where(vav => vav.VariantId == variantId)
                .Include(vav => vav.AttributeValue)
                    .ThenInclude(av => av.Attribute)
                .ToListAsync();
        }

        public async Task<Dictionary<Guid, List<Domain.Entities.Products.VariantAttributeValue>>> GetVariantAttributeValuesBulkAsync(List<Guid> variantIds)
        {
            if (variantIds == null || !variantIds.Any())
                return new Dictionary<Guid, List<Domain.Entities.Products.VariantAttributeValue>>();

            var allValues = await _context.Set<Domain.Entities.Products.VariantAttributeValue>()
                .Where(vav => variantIds.Contains(vav.VariantId))
                .Include(vav => vav.AttributeValue)
                    .ThenInclude(av => av.Attribute)
                .ToListAsync();

            return allValues
                .GroupBy(vav => vav.VariantId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        public async Task AssignVariantAttributeValueAsync(Domain.Entities.Products.VariantAttributeValue vav)
        {
            // Determine attribute id of the provided attribute value
            var providedValue = await _context.Set<AttributeValue>()
                .FirstOrDefaultAsync(av => av.AttributeValueId == vav.AttributeValueId);

            if (providedValue == null)
                throw new ArgumentException("Attribute value not found");

            // Remove existing attribute value for same attribute if present
            var existing = await _context.Set<Domain.Entities.Products.VariantAttributeValue>()
                .Include(x => x.AttributeValue)
                .FirstOrDefaultAsync(x => x.VariantId == vav.VariantId && x.AttributeValue.AttributeId == providedValue.AttributeId);

            if (existing != null)
                _context.Set<Domain.Entities.Products.VariantAttributeValue>().Remove(existing);

            await _context.Set<Domain.Entities.Products.VariantAttributeValue>().AddAsync(vav);
        }

        public async Task RemoveVariantAttributeValueAsync(Guid variantId, Guid attributeValueId)
        {
            var existing = await _context.Set<Domain.Entities.Products.VariantAttributeValue>()
                .FirstOrDefaultAsync(x => x.VariantId == variantId && x.AttributeValueId == attributeValueId);

            if (existing != null)
                _context.Set<Domain.Entities.Products.VariantAttributeValue>().Remove(existing);
        }


        public async Task AddAsync(ProductAttribute attribute)
        {
            await _context.Set<ProductAttribute>().AddAsync(attribute);
        }

        public async Task AddCategoryAttributeAsync(Domain.Entities.Products.CategoryAttribute ca)
        {
            await _context.Set<Domain.Entities.Products.CategoryAttribute>().AddAsync(ca);
        }

        public async Task RemoveCategoryAttributeAsync(Guid categoryId, Guid attributeId)
        {
            var existing = await _context.Set<Domain.Entities.Products.CategoryAttribute>()
                .FirstOrDefaultAsync(x => x.CategoryId == categoryId && x.AttributeId == attributeId);
            if (existing != null)
                _context.Set<Domain.Entities.Products.CategoryAttribute>().Remove(existing);
        }

        public async Task<List<Guid>> GetCategoriesByAttributeIdAsync(Guid attributeId)
        {
            return await _context.Set<Domain.Entities.Products.CategoryAttribute>()
                .Where(ca => ca.AttributeId == attributeId)
                .Select(ca => ca.CategoryId)
                .ToListAsync();
        }

        public Task UpdateAsync(ProductAttribute attribute)
        {
            _context.Set<ProductAttribute>().Update(attribute);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var item = await GetByIdAsync(id);
            if (item != null)
                _context.Set<ProductAttribute>().Remove(item);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}