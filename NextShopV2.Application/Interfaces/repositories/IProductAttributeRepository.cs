using NextShopV2.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Application.Interfaces.Repositories
{
    public interface IProductAttributeRepository
    {
        Task<List<ProductAttribute>> GetAllAsync();
        Task<ProductAttribute?> GetByIdAsync(Guid id);
        Task<List<ProductAttribute>> GetByCategoryIdAsync(Guid categoryId);
        Task<List<ProductAttribute>> GetByProductIdAsync(Guid productId);

        Task AddCategoryAttributeAsync(Domain.Entities.Products.CategoryAttribute ca);
        Task RemoveCategoryAttributeAsync(Guid categoryId, Guid attributeId);

        Task<List<Guid>> GetCategoriesByAttributeIdAsync(Guid attributeId);

        Task<List<AttributeValue>> GetValuesByAttributeIdAsync(Guid attributeId);
        Task<AttributeValue?> GetValueByIdAsync(Guid id);
        Task AddAttributeValueAsync(AttributeValue value);
        Task UpdateAttributeValueAsync(AttributeValue value);
        Task DeleteAttributeValueAsync(Guid id);

        Task<List<Domain.Entities.Products.VariantAttributeValue>> GetVariantAttributeValuesAsync(Guid variantId);
        Task AssignVariantAttributeValueAsync(Domain.Entities.Products.VariantAttributeValue vav);
        Task RemoveVariantAttributeValueAsync(Guid variantId, Guid attributeValueId);

        Task AddAsync(ProductAttribute attribute);
        Task UpdateAsync(ProductAttribute attribute);
        Task DeleteAsync(Guid id);
        Task SaveAsync();
    }
}