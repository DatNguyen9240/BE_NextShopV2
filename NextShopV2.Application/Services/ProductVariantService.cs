using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Shared.Interfaces;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Domain.Entities.Products;
using NextShopV2.Shared.Extensions;
using NextShopV2.Shared.Helpers;
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
        private readonly IOrderResolutionService _orderResolutionService;

        public ProductVariantService(IProductVariantRepository variantRepo, IProductRepository productRepo, IOrderResolutionService orderResolutionService)
        {
            _variantRepo = variantRepo;
            _productRepo = productRepo;
            _orderResolutionService = orderResolutionService;
        }

        public async Task<List<ProductVariantResponse>> GetAllAsync()
        {
            var variants = await _variantRepo.GetAllAsync();
            
            if (variants.IsNullOrEmpty())
                return new List<ProductVariantResponse>();
                
            return variants.Select(v => new ProductVariantResponse
            {
                ProductVariantId = v.VariantId,
                ProductId = v.ProductId,
                Sku = string.IsNullOrEmpty(v.SKU) 
                    ? CommonHelpers.GenerateSKU("PRD", v.Color, v.Size) 
                    : v.SKU,
                Color = v.Color,
                Size = v.Size,
                // Xoá AdditionalPrice
                StockQuantity = v.StockQuantity,
                IsDefault = v.IsDefault,
                DisplayOrder = v.DisplayOrder,
                ImageUrl = v.ImageUrl,
                ImgHover = v.ImgHover,
                BasePrice = v.BasePrice,
                DiscountPercent = v.DiscountPercent,
                DiscountAmount = v.DiscountAmount,
                PriceAfterDiscount = v.PriceAfterDiscount
            }).ToList();
        }

        public async Task<ProductVariantResponse?> GetByIdAsync(Guid id)
        {
            var variant = await _variantRepo.GetByIdAsync(id);
            if (variant.IsNull())
                return null;

            return new ProductVariantResponse
            {
                ProductVariantId = variant.VariantId,
                ProductId = variant.ProductId,
                Sku = string.IsNullOrEmpty(variant.SKU) 
                    ? CommonHelpers.GenerateSKU("PRD", variant.Color, variant.Size) 
                    : variant.SKU,
                Color = variant.Color,
                Size = variant.Size,
                StockQuantity = variant.StockQuantity,
                IsDefault = variant.IsDefault,
                DisplayOrder = variant.DisplayOrder,
                ImageUrl = variant.ImageUrl,
                ImgHover = variant.ImgHover,
                BasePrice = variant.BasePrice,
                DiscountPercent = variant.DiscountPercent,
                DiscountAmount = variant.DiscountAmount,
                PriceAfterDiscount = variant.PriceAfterDiscount
            };
        }

        public async Task<List<ProductVariantResponse>> GetByProductIdAsync(Guid productId)
        {
            var variants = await _variantRepo.GetByProductIdAsync(productId);
            
            if (variants.IsNullOrEmpty())
                return new List<ProductVariantResponse>();
            
            // Sort by DisplayOrder first, then by VariantId for consistency when DisplayOrder is same
            var sortedVariants = variants
                .OrderBy(v => v.DisplayOrder)
                .ThenBy(v => v.VariantId)
                .ToList();
            
            return sortedVariants.Select(v => new ProductVariantResponse
            {
                ProductVariantId = v.VariantId,
                ProductId = v.ProductId,
                Sku = string.IsNullOrEmpty(v.SKU) 
                    ? CommonHelpers.GenerateSKU("PRD", v.Color, v.Size) 
                    : v.SKU,
                Color = v.Color,
                Size = v.Size,
            
                StockQuantity = v.StockQuantity,
                IsDefault = v.IsDefault,
                DisplayOrder = v.DisplayOrder,
                ImageUrl = v.ImageUrl,
                ImgHover = v.ImgHover
            }).ToList();
        }

        public async Task<ProductVariantResponse?> GetDefaultByProductIdAsync(Guid productId)
        {
            var variant = await _variantRepo.GetDefaultByProductIdAsync(productId);
            if (variant.IsNull())
                return null;

            return new ProductVariantResponse
            {
                ProductVariantId = variant!.VariantId,
                ProductId = variant.ProductId,
                Sku = string.IsNullOrEmpty(variant.SKU) 
                    ? CommonHelpers.GenerateSKU("PRD", variant.Color, variant.Size) 
                    : variant.SKU,
                Color = variant.Color,
                Size = variant.Size,
                // Xoá AdditionalPrice
                StockQuantity = variant.StockQuantity,
                IsDefault = variant.IsDefault,
                DisplayOrder = variant.DisplayOrder,
                ImageUrl = variant.ImageUrl,
                ImgHover = variant.ImgHover
            };
        }

        public async Task<ProductVariantResponse> CreateAsync(CreateProductVariantRequest request)
        {
            // Validate product exists
            if (!await _productRepo.ExistsAsync(request.ProductId))
            {
                throw new ArgumentException("Product not found");
            }

            // Auto-resolve DisplayOrder conflict using OrderResolutionService
            var existingVariants = await _variantRepo.GetByProductIdAsync(request.ProductId);
            var existingDisplayOrders = existingVariants.IsNullOrEmpty() 
                ? new List<int>() 
                : existingVariants.Select(v => v.DisplayOrder);
            var resolvedDisplayOrder = _orderResolutionService.ResolveOrder(existingDisplayOrders, request.DisplayOrder);

            // If this is marked as default, unmark other defaults for this product
            if (request.IsDefault)
            {
                await UnmarkOtherDefaultsAsync(request.ProductId);
            }

            // Auto-generate SKU if not provided
            var generatedSKU = string.IsNullOrEmpty(request.SKU) 
                ? CommonHelpers.GenerateSKU("PRD", request.Color, request.Size)
                : request.SKU;

            // Tính DiscountAmount và PriceAfterDiscount
            decimal discountAmount = 0;
            decimal priceAfterDiscount = 0;
            if (request.BasePrice > 0 && request.DiscountPercent > 0)
            {
                discountAmount = request.BasePrice * request.DiscountPercent / 100;
                priceAfterDiscount = request.BasePrice - discountAmount;
            }
            else
            {
                priceAfterDiscount = request.BasePrice;
            }

            var variant = new ProductVariant
            {
                VariantId = Guid.NewGuid(),
                ProductId = request.ProductId,
                SKU = generatedSKU,
                Color = request.Color,
                Size = request.Size,
                StockQuantity = request.StockQuantity,
                IsDefault = request.IsDefault,
                DisplayOrder = resolvedDisplayOrder, // ← Use resolved DisplayOrder
                ImageUrl = request.ImageUrl,
                ImgHover = request.ImgHover,
                BasePrice = request.BasePrice,
                DiscountPercent = request.DiscountPercent,
                DiscountAmount = discountAmount,
                PriceAfterDiscount = priceAfterDiscount
            };

            await _variantRepo.AddAsync(variant);
            await _variantRepo.SaveAsync();

            return new ProductVariantResponse
            {
                ProductVariantId = variant.VariantId,
                ProductId = variant.ProductId,
                Sku = variant.SKU, // SKU đã được generate trong CreateAsync
                Color = variant.Color,
                Size = variant.Size,
                StockQuantity = variant.StockQuantity,
                IsDefault = variant.IsDefault,
                DisplayOrder = variant.DisplayOrder,
                ImageUrl = variant.ImageUrl,
                ImgHover = variant.ImgHover
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateProductVariantRequest request)
        {
            var variant = await _variantRepo.GetByIdAsync(id);
            if (variant.IsNull()) return false;

            // Auto-resolve DisplayOrder conflict using OrderResolutionService
            var existingVariants = await _variantRepo.GetByProductIdAsync(variant!.ProductId);
            var existingDisplayOrders = existingVariants.IsNullOrEmpty() 
                ? new List<int>() 
                : existingVariants.Select(v => v.DisplayOrder);
            var resolvedDisplayOrder = _orderResolutionService.ResolveOrder(existingDisplayOrders, request.DisplayOrder, variant.DisplayOrder);

            // If marking as default, unmark other defaults for this product
            if (request.IsDefault && !variant.IsDefault)
            {
                await UnmarkOtherDefaultsAsync(variant.ProductId);
            }

            // Auto-generate SKU if empty or null
            if (string.IsNullOrEmpty(request.SKU))
            {
                variant.SKU = CommonHelpers.GenerateSKU("PRD", request.Color, request.Size);
            }
            else
            {
                variant.SKU = request.SKU;
            }
            
            variant.Color = request.Color;
            variant.Size = request.Size;
            // Xoá AdditionalPrice
            variant.StockQuantity = request.StockQuantity;
            variant.IsDefault = request.IsDefault;
            variant.DisplayOrder = resolvedDisplayOrder; // ← Use resolved DisplayOrder
            variant.ImageUrl = request.ImageUrl;
            variant.ImgHover = request.ImgHover;

            await _variantRepo.UpdateAsync(variant);
            await _variantRepo.SaveAsync();

            return true;
        }

        public async Task<bool> UpdateStockAsync(Guid id, UpdateStockRequest request)
        {
            var variant = await _variantRepo.GetByIdAsync(id);
            if (variant.IsNull()) return false;

            variant!.StockQuantity = request.StockQuantity;

            await _variantRepo.UpdateAsync(variant);
            await _variantRepo.SaveAsync();

            return true;
        }

        public async Task<bool> SetAsDefaultAsync(Guid id)
        {
            var variant = await _variantRepo.GetByIdAsync(id);
            if (variant.IsNull()) return false;

            // Unmark other defaults for this product
            await UnmarkOtherDefaultsAsync(variant!.ProductId);

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
            
            if (variants.IsNullOrEmpty()) 
                return;
                
            var defaultVariants = variants.Where(v => v.IsDefault).ToList();

            await defaultVariants.SafeForEachAsync(async variant =>
            {
                variant.IsDefault = false;
                await _variantRepo.UpdateAsync(variant);
            });
        }
    }
}