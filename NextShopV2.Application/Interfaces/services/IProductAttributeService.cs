using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Application.Interfaces.Services
{
    public interface IProductAttributeService
    {
        Task<List<ProductAttributeDto>> GetByProductIdAsync(Guid productId);
        Task<List<ProductAttributeDto>> GetByCategoryIdAsync(Guid categoryId);
        Task<List<ProductAttributeDto>> GetAllAsync();
        Task<ProductAttributeDto?> GetByIdAsync(Guid id);

        Task<AttributeValueDto> CreateAttributeValueAsync(CreateAttributeValueRequest request);
        Task<ProductAttributeDto> CreateAttributeAsync(CreateAttributeRequest request);

        Task<List<AttributeValueDto>> GetValuesByAttributeIdAsync(Guid attributeId);

        // Variant attribute assignments
        Task<List<Guid>> GetVariantAttributeValueIdsAsync(Guid variantId);
        Task AssignVariantAttributeValueAsync(AssignVariantAttributeRequest request);
        Task RemoveVariantAttributeValueAsync(Guid variantId, Guid attributeValueId);

        // Variant attribute map: attribute name -> value
        Task<Dictionary<string, string>> GetVariantAttributeMapAsync(Guid variantId);
        
        // Bulk variant attribute maps: variantId -> (attribute name -> value)
        Task<Dictionary<Guid, Dictionary<string, string>>> GetVariantAttributeMapsAsync(List<Guid> variantIds);

        // Category-attribute assignments
        Task AssignAttributeToCategoryAsync(Guid categoryId, Guid attributeId);
        Task RemoveAttributeFromCategoryAsync(Guid categoryId, Guid attributeId);

        Task<List<Guid>> GetCategoriesByAttributeIdAsync(Guid attributeId);

        // Attribute update/delete
        Task<ProductAttributeDto?> UpdateAttributeAsync(Guid id, DTOs.Request.UpdateAttributeRequest request);
        Task<bool> DeleteAttributeAsync(Guid id);

        // Attribute value update/delete
        Task<AttributeValueDto?> UpdateAttributeValueAsync(Guid id, DTOs.Request.UpdateAttributeValueRequest request);
        Task<bool> DeleteAttributeValueAsync(Guid id);
    }
}