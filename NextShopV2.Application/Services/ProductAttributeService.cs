using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextShopV2.Application.Services
{
    public class ProductAttributeService : IProductAttributeService
    {
        private readonly IProductAttributeRepository _repo;

        public ProductAttributeService(IProductAttributeRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<ProductAttributeDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(a => new ProductAttributeDto {
                AttributeId = a.AttributeId,
                Name = a.Name,
                InputType = a.InputType,
                IsActive = a.IsActive,
                Values = a.Values?.Select(v => new AttributeValueDto {
                    AttributeValueId = v.AttributeValueId,
                    AttributeId = v.AttributeId,
                    Value = v.Value,
                    DisplayOrder = v.DisplayOrder,
                    IsActive = v.IsActive
                }).ToList()
            }).ToList();
        }

        public async Task<ProductAttributeDto?> GetByIdAsync(Guid id)
        {
            var a = await _repo.GetByIdAsync(id);
            if (a == null) return null;
            return new ProductAttributeDto
            {
                AttributeId = a.AttributeId,
                Name = a.Name,
                InputType = a.InputType,
                IsActive = a.IsActive,
                Values = a.Values?.Select(v => new AttributeValueDto {
                    AttributeValueId = v.AttributeValueId,
                    AttributeId = v.AttributeId,
                    Value = v.Value,
                    DisplayOrder = v.DisplayOrder,
                    IsActive = v.IsActive
                }).ToList()
            };
        }

        public async Task<List<ProductAttributeDto>> GetByProductIdAsync(Guid productId)
        {
            var attrs = await _repo.GetByProductIdAsync(productId);
            return attrs.Select(a => new ProductAttributeDto {
                AttributeId = a.AttributeId,
                Name = a.Name,
                InputType = a.InputType,
                IsActive = a.IsActive,
                Values = a.Values?.Select(v => new AttributeValueDto {
                    AttributeValueId = v.AttributeValueId,
                    AttributeId = v.AttributeId,
                    Value = v.Value,
                    DisplayOrder = v.DisplayOrder,
                    IsActive = v.IsActive
                }).ToList()
            }).ToList();
        }

        public async Task<List<ProductAttributeDto>> GetByCategoryIdAsync(Guid categoryId)
        {
            var attrs = await _repo.GetByCategoryIdAsync(categoryId);
            return attrs.Select(a => new ProductAttributeDto {
                AttributeId = a.AttributeId,
                Name = a.Name,
                InputType = a.InputType,
                IsActive = a.IsActive,
                Values = a.Values?.Select(v => new AttributeValueDto {
                    AttributeValueId = v.AttributeValueId,
                    AttributeId = v.AttributeId,
                    Value = v.Value,
                    DisplayOrder = v.DisplayOrder,
                    IsActive = v.IsActive
                }).ToList()
            }).ToList();
        }

        public async Task<AttributeValueDto> CreateAttributeValueAsync(CreateAttributeValueRequest request)
        {
            var value = new AttributeValue
            {
                AttributeValueId = Guid.NewGuid(),
                AttributeId = request.AttributeId,
                Value = request.Value,
                DisplayOrder = request.DisplayOrder,
                IsActive = request.IsActive
            };
            await _repo.AddAttributeValueAsync(value);
            await _repo.SaveAsync();

            return new AttributeValueDto
            {
                AttributeValueId = value.AttributeValueId,
                AttributeId = value.AttributeId,
                Value = value.Value,
                DisplayOrder = value.DisplayOrder,
                IsActive = value.IsActive
            };
        }

        public async Task<AttributeValueDto?> UpdateAttributeValueAsync(Guid id, UpdateAttributeValueRequest request)
        {
            var v = await _repo.GetValueByIdAsync(id);
            if (v == null) return null;
            v.Value = request.Value;
            v.DisplayOrder = request.DisplayOrder;
            v.IsActive = request.IsActive;
            await _repo.UpdateAttributeValueAsync(v);
            await _repo.SaveAsync();

            return new AttributeValueDto
            {
                AttributeValueId = v.AttributeValueId,
                AttributeId = v.AttributeId,
                Value = v.Value,
                DisplayOrder = v.DisplayOrder,
                IsActive = v.IsActive
            };
        }

        public async Task<bool> DeleteAttributeValueAsync(Guid id)
        {
            var v = await _repo.GetValueByIdAsync(id);
            if (v == null) return false;
            await _repo.DeleteAttributeValueAsync(id);
            await _repo.SaveAsync();
            return true;
        }

        public async Task<ProductAttributeDto> CreateAttributeAsync(CreateAttributeRequest request)
        {
            var a = new ProductAttribute
            {
                AttributeId = Guid.NewGuid(),
                Name = request.Name,
                InputType = request.InputType,
                IsActive = request.IsActive
            };
            await _repo.AddAsync(a);
            await _repo.SaveAsync();

            return new ProductAttributeDto
            {
                AttributeId = a.AttributeId,
                Name = a.Name,
                InputType = a.InputType,
                IsActive = a.IsActive
            };
        }

        public async Task<ProductAttributeDto?> UpdateAttributeAsync(Guid id, UpdateAttributeRequest request)
        {
            var a = await _repo.GetByIdAsync(id);
            if (a == null) return null;
            a.Name = request.Name;
            a.InputType = request.InputType;
            a.IsActive = request.IsActive;
            await _repo.UpdateAsync(a);
            await _repo.SaveAsync();

            return new ProductAttributeDto
            {
                AttributeId = a.AttributeId,
                Name = a.Name,
                InputType = a.InputType,
                IsActive = a.IsActive,
                Values = a.Values?.Select(v => new AttributeValueDto {
                    AttributeValueId = v.AttributeValueId,
                    AttributeId = v.AttributeId,
                    Value = v.Value,
                    DisplayOrder = v.DisplayOrder,
                    IsActive = v.IsActive
                }).ToList()
            };
        }

        public async Task<bool> DeleteAttributeAsync(Guid id)
        {
            var a = await _repo.GetByIdAsync(id);
            if (a == null) return false;

          
            await _repo.DeleteAsync(id);
            await _repo.SaveAsync();
            return true;
        }

        public async Task<List<AttributeValueDto>> GetValuesByAttributeIdAsync(Guid attributeId)
        {
            var values = await _repo.GetValuesByAttributeIdAsync(attributeId);
            return values.Select(v => new AttributeValueDto
            {
                AttributeValueId = v.AttributeValueId,
                AttributeId = v.AttributeId,
                Value = v.Value,
                DisplayOrder = v.DisplayOrder,
                IsActive = v.IsActive
            }).ToList();
        }

        public async Task<List<Guid>> GetVariantAttributeValueIdsAsync(Guid variantId)
        {
            var vavs = await _repo.GetVariantAttributeValuesAsync(variantId);
            return vavs.Select(v => v.AttributeValueId).ToList();
        }

        public async Task<Dictionary<string, string>> GetVariantAttributeMapAsync(Guid variantId)
        {
            var vavs = await _repo.GetVariantAttributeValuesAsync(variantId);
            var dict = new Dictionary<string, string>();
            foreach (var vav in vavs)
            {
                var attrName = vav.AttributeValue?.Attribute?.Name ?? string.Empty;
                var value = vav.AttributeValue?.Value ?? string.Empty;
                if (!string.IsNullOrEmpty(attrName))
                    dict[attrName] = value;
            }
            return dict;
        }

        public async Task AssignVariantAttributeValueAsync(AssignVariantAttributeRequest request)
        {
            var value = await _repo.GetValueByIdAsync(request.AttributeValueId);
            if (value == null) throw new ArgumentException("Attribute value not found");

            var vav = new VariantAttributeValue
            {
                VariantId = request.VariantId,
                AttributeValueId = request.AttributeValueId
            };

            await _repo.AssignVariantAttributeValueAsync(vav);
            await _repo.SaveAsync();
        }

        public async Task RemoveVariantAttributeValueAsync(Guid variantId, Guid attributeValueId)
        {
            await _repo.RemoveVariantAttributeValueAsync(variantId, attributeValueId);
            await _repo.SaveAsync();
        }

        public async Task AssignAttributeToCategoryAsync(Guid categoryId, Guid attributeId)
        {
            var ca = new Domain.Entities.Products.CategoryAttribute
            {
                CategoryId = categoryId,
                AttributeId = attributeId,
                AssignedAt = DateTime.UtcNow
            };
            await _repo.AddCategoryAttributeAsync(ca);
            await _repo.SaveAsync();
        }

        public async Task RemoveAttributeFromCategoryAsync(Guid categoryId, Guid attributeId)
        {
            await _repo.RemoveCategoryAttributeAsync(categoryId, attributeId);
            await _repo.SaveAsync();
        }

        public async Task<List<Guid>> GetCategoriesByAttributeIdAsync(Guid attributeId)
        {
            return await _repo.GetCategoriesByAttributeIdAsync(attributeId);
        }
    }
}