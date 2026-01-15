using NextShopV2.Domain.Entities.Products;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Application.DTOs.Request;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllAsync(bool includeInactive = false);
        Task<PagedResult<ProductDto>> GetPagedAsync(Guid? categoryId = null, int page = 1, int pageSize = 12, decimal? minPrice = null, decimal? maxPrice = null, string? sort = null, bool includeInactive = false);
        Task<ProductDto?> GetByIdAsync(Guid id, bool includeInactive = false);
        Task<ProductDto> CreateAsync(CreateProductRequest request);
        Task<bool> UpdateAsync(Guid id, UpdateProductRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}
