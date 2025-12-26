using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.Interfaces.services;

using NextShopV2.Application.DTOs.Response;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Domain.Entities.Products;
using NextShopV2.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace NextShopV2.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly IProductCategoryService _productCategoryService;
        public ProductService(IProductRepository repo, IProductCategoryService productCategoryService)
        {
            _repo = repo;
            _productCategoryService = productCategoryService;
        }
        public async Task<List<ProductDto>> GetAllAsync()
        {
            var products = await _repo.GetAllAsync();
            return products.Select(p => p.ToDto()).ToList();
        }

        public async Task<PagedResult<ProductDto>> GetBySectionAsync(string? section, Guid? categoryId = null, int page = 1, int pageSize = 12)
        {
            // For now, load all and filter in-memory. For large datasets, implement repository queries.
            var products = (await _repo.GetAllAsync()).AsQueryable();

            // Filter by category if provided
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId.Value));
            }

            // Normalize section param. Treat `section` AS the section name and filter by tag `section:{name}`.
            var s = section?.Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(s))
            {
                var tagKey = $"section:{s}";
                var tagged = products.Where(p => (p.Tags ?? new List<string>()).Any(t => t != null && string.Equals(t.Trim(), tagKey, StringComparison.OrdinalIgnoreCase)));

                // Only return tagged products for the requested section; if none found, return empty result set.
                products = tagged;
            }

            var total = products.Count();
            var totalPages = (int)System.Math.Ceiling((double)total / pageSize);
            var items = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<ProductDto>
            {
                Items = items.Select(p => p.ToDto()).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = totalPages
            };
        }

        public async Task<ProductDto?> GetByIdAsync(Guid id)
        {
            var product = await _repo.GetByIdAsync(id);
            return product?.ToDto();
        }
        public async Task<ProductDto> CreateAsync(CreateProductRequest request)
        {
            var product = new Product
            {
                ProductId = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                GenderTarget = request.GenderTarget,
                Brand = request.Brand,
                AverageRating = 0,
                TotalReviews = 0,
                TotalLikes = 0,
                CreatedAt = DateTime.UtcNow,
                IsActive = request.IsActive,
                Tags = request.Tags ?? new List<string>()
            };
            await _repo.AddAsync(product);
            await _repo.SaveAsync();

            // If categories were provided, bulk assign them to the created product
            if (request.CategoryIds != null && request.CategoryIds.Any())
            {
                await _productCategoryService.BulkAssignProductToCategoriesAsync(new BulkAssignProductToCategoriesRequest
                {
                    ProductId = product.ProductId,
                    CategoryIds = request.CategoryIds
                });
            }

            // reload product with categories if needed
            var created = await _repo.GetByIdAsync(product.ProductId);
            return created.ToDto();
        }
        public async Task<bool> UpdateAsync(Guid id, UpdateProductRequest request)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product.IsNull()) return false;
            
            product!.Name = request.Name;
            product.Description = request.Description;
            product.GenderTarget = request.GenderTarget;
            product.Brand = request.Brand;
            product.IsActive = request.IsActive;
            product.Tags = request.Tags ?? new List<string>();
            await _repo.UpdateAsync(product);
            await _repo.SaveAsync();

            // If categoryIds provided in update, replace existing assignments
            if (request.CategoryIds != null)
            {
                await _productCategoryService.BulkAssignProductToCategoriesAsync(new BulkAssignProductToCategoriesRequest
                {
                    ProductId = product.ProductId,
                    CategoryIds = request.CategoryIds
                });
            }

            return true;
        }
        public async Task<bool> DeleteAsync(Guid id)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product.IsNull()) return false;
            
            await _repo.DeleteAsync(product!);
            await _repo.SaveAsync();
            return true;
        }
    }
    public static class ProductMapping
    {
        public static ProductDto ToDto(this Product p)
        {
            return new ProductDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                GenderTarget = p.GenderTarget,
                Brand = p.Brand,
                AverageRating = p.AverageRating,
                TotalReviews = p.TotalReviews,
                TotalLikes = p.TotalLikes,
                IsActive = p.IsActive,
                Tags = p.Tags ?? new List<string>(),
                Variants = p.Variants.IsNullOrEmpty() 
                    ? new List<ProductVariantResponse>()
                    : p.Variants
                        .OrderBy(v => v.DisplayOrder)
                        .ThenBy(v => v.VariantId)
                        .Select(v => new ProductVariantResponse
                        {
                            ProductVariantId = v.VariantId,
                            ProductId = v.ProductId,
                            Sku = v.SKU,
                            Color = v.Color,
                            Size = v.Size,
                            StockQuantity = v.StockQuantity,
                            IsDefault = v.IsDefault,
                            DisplayOrder = v.DisplayOrder,
                            ImageUrl = v.ImageUrl,
                            ImgHover = v.ImgHover,
                            BasePrice = v.BasePrice,
                            DiscountPercent = v.DiscountPercent,
                            DiscountAmount = v.DiscountAmount,
                            PriceAfterDiscount = v.PriceAfterDiscount
                        }).ToList(),
                CategoryIds = p.ProductCategories.IsNullOrEmpty() ? new List<Guid>() : p.ProductCategories.Select(pc => pc.CategoryId).ToList()
            };
        }
    }
}
