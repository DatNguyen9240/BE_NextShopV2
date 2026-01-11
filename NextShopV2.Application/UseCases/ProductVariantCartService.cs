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

        public ProductVariantCartService(IProductVariantRepository variantRepository)
        {
            _variantRepository = variantRepository;
        }

        public async Task<VariantInfo?> GetVariantInfoAsync(Guid variantId)
        {
            var variant = await _variantRepository.GetByIdAsync(variantId);
            if (variant == null)
                return null;

            return new VariantInfo
            {
                Price = variant.PriceAfterDiscount,
                ProductName = variant.Product?.Name ?? string.Empty,
                Color = variant.Color ?? string.Empty,
                Size = variant.Size ?? string.Empty,
                ImageUrl = variant.ImageUrl ?? string.Empty,
                Sku = string.IsNullOrEmpty(variant.SKU) 
                    ? CommonHelpers.GenerateSKU("PRD", variant.Color ?? string.Empty, variant.Size ?? string.Empty) 
                    : variant.SKU,
                StockQuantity = variant.StockQuantity
            };
        }
    }
}