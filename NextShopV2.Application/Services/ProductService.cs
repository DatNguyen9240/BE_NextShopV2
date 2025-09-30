using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace NextShopV2.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        public ProductService(IProductRepository repo)
        {
            _repo = repo;
        }
        public async Task<List<ProductDto>> GetAllAsync()
        {
            var products = await _repo.GetAllAsync();
            return products.Select(p => p.ToDto()).ToList();
        }
        public async Task<ProductDto?> GetByIdAsync(Guid id)
        {
            var product = await _repo.GetByIdAsync(id);
            return product?.ToDto();
        }
        public async Task<ProductDto> CreateAsync(ProductDto dto)
        {
            var product = new Product
            {
                ProductId = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                BasePrice = dto.BasePrice,
                GenderTarget = dto.GenderTarget,
                Brand = dto.Brand,
                AverageRating = 0,
                TotalReviews = 0,
                TotalLikes = 0,
                CreatedAt = DateTime.UtcNow,
                IsActive = dto.IsActive
            };
            await _repo.AddAsync(product);
            await _repo.SaveAsync();
            return product.ToDto();
        }
        public async Task<bool> UpdateAsync(Guid id, ProductDto dto)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null) return false;
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.BasePrice = dto.BasePrice;
            product.GenderTarget = dto.GenderTarget;
            product.Brand = dto.Brand;
            product.IsActive = dto.IsActive;
            await _repo.UpdateAsync(product);
            await _repo.SaveAsync();
            return true;
        }
        public async Task<bool> DeleteAsync(Guid id)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null) return false;
            await _repo.DeleteAsync(product);
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
                BasePrice = p.BasePrice,
                GenderTarget = p.GenderTarget,
                Brand = p.Brand,
                AverageRating = p.AverageRating,
                TotalReviews = p.TotalReviews,
                TotalLikes = p.TotalLikes,
                IsActive = p.IsActive
            };
        }
    }
}
