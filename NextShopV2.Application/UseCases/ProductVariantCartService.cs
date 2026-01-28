using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Shared.Helpers;
using System;
using System.Threading.Tasks;

namespace NextShopV2.Application.UseCases
{
    public class ProductVariantCartService : IProductVariantCartService
    {
        private readonly IProductVariantRepository _variantRepository;
        private readonly IProductAttributeService _attributeService;

        public ProductVariantCartService(IProductVariantRepository variantRepository, IProductAttributeService attributeService)
        {
            _variantRepository = variantRepository;
            _attributeService = attributeService;
        }

        public async Task<VariantInfo?> GetVariantInfoAsync(Guid variantId)
        {
            var variant = await _variantRepository.GetByIdAsync(variantId);
            if (variant == null)
                return null;

            var attributes = await _attributeService.GetVariantAttributeMapAsync(variantId);

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
                ProductIsActive = variant.Product?.IsActive ?? true
            };
        }
    }
}