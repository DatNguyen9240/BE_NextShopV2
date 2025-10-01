using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextShopV2.Application.Services
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IProductVariantRepository _variantRepo;
        private readonly IProductRepository _productRepo;

        public ProductVariantService(IProductVariantRepository variantRepo, IProductRepository productRepo)
        {
            _variantRepo = variantRepo;
            _productRepo = productRepo;
        }

        public async Task<List<ProductVariantResponse>> GetAllAsync()
        {
            var variants = await _variantRepo.GetAllAsync();
            return variants.Select(v => new ProductVariantResponse
            {
                ProductVariantId = v.VariantId,
                ProductId = v.ProductId,
                Color = v.Color,
                Size = v.Size,
                AdditionalPrice = v.AdditionalPrice,
                StockQuantity = v.StockQuantity,
                IsDefault = v.IsDefault,
                DisplayOrder = v.DisplayOrder,
                ImageUrl = v.ImageUrl
            }).ToList();
        }

        public async Task<ProductVariantResponse?> GetByIdAsync(Guid id)
        {
            var variant = await _variantRepo.GetByIdAsync(id);
            return variant == null ? null : new ProductVariantResponse
            {
                ProductVariantId = variant.VariantId,
                ProductId = variant.ProductId,
                Color = variant.Color,
                Size = variant.Size,
                AdditionalPrice = variant.AdditionalPrice,
                StockQuantity = variant.StockQuantity,
                IsDefault = variant.IsDefault,
                DisplayOrder = variant.DisplayOrder,
                ImageUrl = variant.ImageUrl
            };
        }

        public async Task<List<ProductVariantResponse>> GetByProductIdAsync(Guid productId)
        {
            var variants = await _variantRepo.GetByProductIdAsync(productId);
            return variants.Select(v => new ProductVariantResponse
            {
                ProductVariantId = v.VariantId,
                ProductId = v.ProductId,
                Color = v.Color,
                Size = v.Size,
                AdditionalPrice = v.AdditionalPrice,
                StockQuantity = v.StockQuantity,
                IsDefault = v.IsDefault,
                DisplayOrder = v.DisplayOrder,
                ImageUrl = v.ImageUrl
            }).ToList();
        }

        public async Task<ProductVariantResponse?> GetDefaultByProductIdAsync(Guid productId)
        {
            var variant = await _variantRepo.GetDefaultByProductIdAsync(productId);
            return variant == null ? null : new ProductVariantResponse
            {
                ProductVariantId = variant.VariantId,
                ProductId = variant.ProductId,
                Color = variant.Color,
                Size = variant.Size,
                AdditionalPrice = variant.AdditionalPrice,
                StockQuantity = variant.StockQuantity,
                IsDefault = variant.IsDefault,
                DisplayOrder = variant.DisplayOrder,
                ImageUrl = variant.ImageUrl
            };
        }

        public async Task<ProductVariantResponse> CreateAsync(CreateProductVariantRequest request)
        {
            // Validate product exists
            if (!await _productRepo.ExistsAsync(request.ProductId))
            {
                throw new ArgumentException("Product not found");
            }

            // If this is marked as default, unmark other defaults for this product
            if (request.IsDefault)
            {
                await UnmarkOtherDefaultsAsync(request.ProductId);
            }

            var variant = new ProductVariant
            {
                VariantId = Guid.NewGuid(),
                ProductId = request.ProductId,
                SKU = request.SKU,
                Color = request.Color,
                Size = request.Size,
                AdditionalPrice = request.AdditionalPrice,
                StockQuantity = request.StockQuantity,
                IsDefault = request.IsDefault,
                DisplayOrder = request.DisplayOrder,
                ImageUrl = request.ImageUrl
            };

            await _variantRepo.AddAsync(variant);
            await _variantRepo.SaveAsync();

            return new ProductVariantResponse
            {
                ProductVariantId = variant.VariantId,
                ProductId = variant.ProductId,
                Color = variant.Color,
                Size = variant.Size,
                AdditionalPrice = variant.AdditionalPrice,
                StockQuantity = variant.StockQuantity,
                IsDefault = variant.IsDefault,
                DisplayOrder = variant.DisplayOrder,
                ImageUrl = variant.ImageUrl
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateProductVariantRequest request)
        {
            var variant = await _variantRepo.GetByIdAsync(id);
            if (variant == null) return false;

            // If marking as default, unmark other defaults for this product
            if (request.IsDefault && !variant.IsDefault)
            {
                await UnmarkOtherDefaultsAsync(variant.ProductId);
            }

            variant.SKU = request.SKU;
            variant.Color = request.Color;
            variant.Size = request.Size;
            variant.AdditionalPrice = request.AdditionalPrice;
            variant.StockQuantity = request.StockQuantity;
            variant.IsDefault = request.IsDefault;
            variant.DisplayOrder = request.DisplayOrder;
            variant.ImageUrl = request.ImageUrl;

            await _variantRepo.UpdateAsync(variant);
            await _variantRepo.SaveAsync();

            return true;
        }

        public async Task<bool> UpdateStockAsync(Guid id, UpdateStockRequest request)
        {
            var variant = await _variantRepo.GetByIdAsync(id);
            if (variant == null) return false;

            variant.StockQuantity = request.StockQuantity;

            await _variantRepo.UpdateAsync(variant);
            await _variantRepo.SaveAsync();

            return true;
        }

        public async Task<bool> SetAsDefaultAsync(Guid id)
        {
            var variant = await _variantRepo.GetByIdAsync(id);
            if (variant == null) return false;

            // Unmark other defaults for this product
            await UnmarkOtherDefaultsAsync(variant.ProductId);

            // Mark this as default
            variant.IsDefault = true;

            await _variantRepo.UpdateAsync(variant);
            await _variantRepo.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!await _variantRepo.ExistsAsync(id)) return false;

            await _variantRepo.DeleteAsync(id);
            await _variantRepo.SaveAsync();

            return true;
        }

        private async Task UnmarkOtherDefaultsAsync(Guid productId)
        {
            var variants = await _variantRepo.GetByProductIdAsync(productId);
            var defaultVariants = variants.Where(v => v.IsDefault).ToList();

            foreach (var variant in defaultVariants)
            {
                variant.IsDefault = false;
                await _variantRepo.UpdateAsync(variant);
            }
        }
    }
}