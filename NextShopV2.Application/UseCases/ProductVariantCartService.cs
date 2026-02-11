using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Shared.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NextShopV2.Application.UseCases
{
    public class ProductVariantCartService : IProductVariantCartService
    {
        private readonly IProductVariantRepository _variantRepository;
        private readonly IProductAttributeService _attributeService;
        private readonly ITaxSettingService _taxSettingService;

        public ProductVariantCartService(
            IProductVariantRepository variantRepository, 
            IProductAttributeService attributeService,
            ITaxSettingService taxSettingService)
        {
            _variantRepository = variantRepository;
            _attributeService = attributeService;
            _taxSettingService = taxSettingService;
        }

        public async Task<VariantInfo?> GetVariantInfoAsync(Guid variantId)
        {
            var variant = await _variantRepository.GetByIdAsync(variantId);
            if (variant == null)
                return null;

            var attributes = await _attributeService.GetVariantAttributeMapAsync(variantId);
            
            // Determine tax rate: Product > Category > System default
            var taxRate = await GetTaxRateAsync(variant.Product);

            return new VariantInfo
            {
                ProductId = variant.ProductId,
                Price = variant.PriceAfterDiscount,
                ProductName = variant.Product?.Name ?? string.Empty,
                Attributes = attributes,
                ImageUrl = variant.ImageUrl ?? string.Empty,
                Sku = string.IsNullOrEmpty(variant.SKU) 
                    ? CommonHelpers.GenerateSKU("PRD") 
                    : variant.SKU,
                StockQuantity = variant.StockQuantity,
                IsActive = variant.IsActive,
                ProductIsActive = variant.Product?.IsActive ?? true,
                TaxRate = taxRate
            };
        }

        public async Task<Dictionary<Guid, VariantInfo>> GetVariantInfosAsync(IEnumerable<Guid> variantIds)
        {
            var result = new Dictionary<Guid, VariantInfo>();
            if (variantIds == null) return result;

            var ids = variantIds.Distinct().Where(id => id != Guid.Empty).ToList();
            if (!ids.Any()) return result;

            var variants = await _variantRepository.GetByIdsAsync(ids);
            var attributeMaps = await _attributeService.GetVariantAttributeMapsAsync(ids);

            foreach (var v in variants)
            {
                var attributes = attributeMaps.TryGetValue(v.VariantId, out var map) ? map : new System.Collections.Generic.Dictionary<string, string>();
                var taxRate = await GetTaxRateAsync(v.Product);

                result[v.VariantId] = new VariantInfo
                {
                    ProductId = v.ProductId,
                    Price = v.PriceAfterDiscount,
                    ProductName = v.Product?.Name ?? string.Empty,
                    Attributes = attributes,
                    ImageUrl = v.ImageUrl ?? string.Empty,
                    Sku = string.IsNullOrEmpty(v.SKU) ? CommonHelpers.GenerateSKU("PRD") : v.SKU,
                    StockQuantity = v.StockQuantity,
                    IsActive = v.IsActive,
                    ProductIsActive = v.Product?.IsActive ?? true,
                    TaxRate = taxRate
                };
            }

            return result;
        }

        private async Task<decimal> GetTaxRateAsync(Domain.Entities.Products.Product? product)
        {
            if (product == null)
                return await _taxSettingService.GetTaxRateAsync();

            // 1. Check product-specific tax rate
            if (product.TaxRate.HasValue)
                return product.TaxRate.Value;

            // 2. Check category tax rate
            var primaryCategory = product.ProductCategories?.FirstOrDefault()?.Category;
            if (primaryCategory != null && primaryCategory.TaxRate.HasValue)
                return primaryCategory.TaxRate.Value;

            // 3. Fall back to system default
            return await _taxSettingService.GetTaxRateAsync();
        }
    }
}