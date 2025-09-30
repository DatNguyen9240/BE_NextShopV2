using NextShopV2.Domain.Entities.Products;
using NextShopV2.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(Guid id);
        Task<ProductDto> CreateAsync(ProductDto dto);
        Task<bool> UpdateAsync(Guid id, ProductDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
