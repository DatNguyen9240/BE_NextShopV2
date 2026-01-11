using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Application.Interfaces.Services
{
    public interface IProductVariantService
    {
        Task<List<ProductVariantResponse>> GetAllAsync();
        Task<ProductVariantResponse?> GetByIdAsync(Guid id);
        Task<List<ProductVariantResponse>> GetByProductIdAsync(Guid productId);
        Task<ProductVariantResponse?> GetDefaultByProductIdAsync(Guid productId);
        Task<ProductVariantResponse> CreateAsync(CreateProductVariantRequest request);
        Task<bool> UpdateAsync(Guid id, UpdateProductVariantRequest request);
        Task<bool> UpdateStockAsync(Guid id, UpdateStockRequest request);
        Task<bool> SetAsDefaultAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
        Task<VariantInfo?> GetVariantInfoAsync(Guid variantId);
    }
}