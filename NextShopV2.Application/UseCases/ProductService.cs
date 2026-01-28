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
        private readonly IProductAttributeService _attributeService;
        public ProductService(IProductRepository repo, IProductCategoryService productCategoryService, IProductAttributeService attributeService)
        {
            _repo = repo;
            _productCategoryService = productCategoryService;
            _attributeService = attributeService;
        }

        // Interface-compatible methods (default to excluding inactive items for store endpoints)
        public Task<List<ProductDto>> GetAllAsync()
        {
            return GetAllAsync(includeInactive: false);
        }

        public Task<PagedResult<ProductDto>> GetPagedAsync(Guid? categoryId = null, int page = 1, int pageSize = 12, decimal? minPrice = null, decimal? maxPrice = null, string? sort = null)
        {
            return GetPagedAsync(categoryId, page, pageSize, minPrice, maxPrice, sort, includeInactive: false);
        }

        public Task<ProductDto?> GetByIdAsync(Guid id)
        {
            return GetByIdAsync(id, includeInactive: false);
        }

        // Extended implementations with includeInactive option
        public async Task<List<ProductDto>> GetAllAsync(bool includeInactive = false)
        {
            var products = await _repo.GetAllAsync();
            if (!includeInactive)
            {
                // Only active products
                products = products.Where(p => p.IsActive).ToList();
                // Exclude products that have no active variants (public store should not list products without available active variants)
                products = products.Where(p => (p.Variants ?? new List<ProductVariant>()).Any(v => v.IsActive)).ToList();
            }
            // For listing endpoints return only the default/representative variant
            return products.Select(p => p.ToDto(includeVariants: false, includeInactiveVariants: includeInactive, includeInactiveProducts: includeInactive)).ToList();
        }

        public async Task<PagedResult<ProductDto>> GetPagedAsync(Guid? categoryId = null, int page = 1, int pageSize = 12, decimal? minPrice = null, decimal? maxPrice = null, string? sort = null, bool includeInactive = false)
        {
            // For now, load all and filter in-memory. For large datasets, implement repository queries.
            var products = (await _repo.GetAllAsync()).AsQueryable();

            // Filter by category if provided
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId.Value));
            }

            // Filter out inactive products for public store unless explicitly requested
            if (!includeInactive)
            {
                products = products.Where(p => p.IsActive);

                // Exclude products that have no active variants after filtering - public store should not list these
                products = products.Where(p => (p.Variants ?? new List<ProductVariant>()).Any(v => v.IsActive));
            }



            // Price filtering across variants (use PriceAfterDiscount if available)
            if (minPrice.HasValue)
            {
                products = products.Where(p => (p.Variants ?? new List<ProductVariant>()).Any(v => (v.PriceAfterDiscount >= minPrice.Value)));
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => (p.Variants ?? new List<ProductVariant>()).Any(v => (v.PriceAfterDiscount <= maxPrice.Value)));
            }

            // Sorting
            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort)
                {
                    case "price_asc":
                        products = products.OrderBy(p => (p.Variants ?? new List<ProductVariant>()).OrderBy(v => v.PriceAfterDiscount).Select(v => v.PriceAfterDiscount).FirstOrDefault());
                        break;
                    case "price_desc":
                        products = products.OrderByDescending(p => (p.Variants ?? new List<ProductVariant>()).OrderBy(v => v.PriceAfterDiscount).Select(v => v.PriceAfterDiscount).FirstOrDefault());
                        break;
                    case "rating_asc":
                        products = products.OrderBy(p => p.AverageRating);
                        break;
                    case "rating_desc":
                        products = products.OrderByDescending(p => p.AverageRating);
                        break;
                    default:
                        break;
                }
            }

            var total = products.Count();
            var totalPages = (int)System.Math.Ceiling((double)total / pageSize);
            var items = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<ProductDto>
            {
                Items = items.Select(p => p.ToDto(includeVariants: false, includeInactiveVariants: includeInactive, includeInactiveProducts: includeInactive)).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = totalPages
            };
        }

        public async Task<ProductDto?> GetByIdAsync(Guid id, bool includeInactive = false)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null) return null;
            if (!includeInactive && !product.IsActive) return null;

            var dto = product.ToDto(includeVariants: true, includeInactiveVariants: includeInactive, includeInactiveProducts: includeInactive);

            // populate variant attributes for frontend convenience (may result in N+1 queries for variants)
            if (dto?.Variants != null && dto.Variants.Count > 0)
            {
                foreach (var v in dto.Variants)
                {
                    try
                    {
                        var map = await _attributeService.GetVariantAttributeMapAsync(v.ProductVariantId);
                        if (map != null && map.Count > 0) v.Attributes = map;
                    }
                    catch
                    {
                        // ignore errors and continue
                    }
                }
            }

            return dto;
        }
        public async Task<ProductDto> CreateAsync(CreateProductRequest request)
        {
            var product = new Product
            {
                ProductId = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                AdditionalInfo = request.AdditionalInfo,
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
            var created = await _repo.GetByIdAsync(product.ProductId) ?? throw new InvalidOperationException("Created product not found after save.");
            return created.ToDto();
        }
        public async Task<bool> UpdateAsync(Guid id, UpdateProductRequest request)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product.IsNull()) return false;
            
            product!.Name = request.Name;
            product.Description = request.Description;
            product.AdditionalInfo = request.AdditionalInfo;
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
        public static ProductDto ToDto(this Product p, bool includeVariants = true, bool includeInactiveVariants = false, bool includeInactiveProducts = false)
        {
            var orderedVariants = p.Variants.IsNullOrEmpty() ? new List<ProductVariant>() : p.Variants.OrderBy(v => v.DisplayOrder).ThenBy(v => v.VariantId).ToList();
            if (!includeInactiveVariants)
            {
                orderedVariants = orderedVariants.Where(v => v.IsActive).ToList();
            }
            var defaultVariant = orderedVariants.FirstOrDefault(v => v.IsDefault) ?? (orderedVariants.Count > 0 ? orderedVariants[0] : null);

            var variantsList = new List<ProductVariantResponse>();

            if (includeVariants)
            {
                variantsList = orderedVariants.Select(v => new ProductVariantResponse
                {
                    ProductVariantId = v.VariantId,
                    Sku = v.SKU,
                    StockQuantity = v.StockQuantity,
                    IsDefault = v.IsDefault,
                    DisplayOrder = v.DisplayOrder,
                    ImageUrl = v.ImageUrl,
                    ImgHover = string.IsNullOrEmpty(v.ImgHover) ? v.ImageUrl : v.ImgHover,
                    BasePrice = v.BasePrice,
                    DiscountPercent = v.DiscountPercent,
                    DiscountAmount = v.DiscountAmount,
                    PriceAfterDiscount = v.PriceAfterDiscount,
                    IsActive = includeInactiveVariants ? v.IsActive : (v.IsActive ? true : (bool?)null)
                }).ToList();
            }
            else
            {
                if (defaultVariant != null)
                {
                    variantsList.Add(new ProductVariantResponse
                    {
                        ProductVariantId = defaultVariant.VariantId,
                        ImageUrl = defaultVariant.ImageUrl,
                        ImgHover = string.IsNullOrEmpty(defaultVariant.ImgHover) ? defaultVariant.ImageUrl : defaultVariant.ImgHover,
                        StockQuantity = defaultVariant.StockQuantity,
                        BasePrice = defaultVariant.BasePrice,
                        PriceAfterDiscount = defaultVariant.PriceAfterDiscount,
                        DiscountPercent = defaultVariant.DiscountPercent,
                        IsActive = includeInactiveVariants ? defaultVariant.IsActive : (defaultVariant.IsActive ? true : (bool?)null),
                        IsDefault = true
                    });
                }
            }

            return new ProductDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                AdditionalInfo = string.IsNullOrWhiteSpace(p.AdditionalInfo) ? null : p.AdditionalInfo,
                GenderTarget = p.GenderTarget,
                Brand = p.Brand,
                AverageRating = p.AverageRating,
                TotalReviews = p.TotalReviews,
                TotalLikes = p.TotalLikes,
                IsActive = includeInactiveProducts ? p.IsActive : (p.IsActive ? true : (bool?)null),
                Tags = (p.Tags == null || !p.Tags.Any()) ? null : p.Tags,
                // Total stock across all variants
                TotalStockQuantity = orderedVariants.Sum(v => v.StockQuantity),
                Variants = variantsList.Count == 0 ? null : variantsList
            };
        }
    }
}
