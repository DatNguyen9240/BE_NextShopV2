using NextShopV2.Application.DTOs;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Application.DTOs.Request.CreateDto;
using NextShopV2.Application.DTOs.Request.UpdateDto;
using System;
using System.Threading.Tasks;

namespace NextShopV2.Application.Interfaces.Services
{
    public interface IRedisCartService
    {
        Task<CartDto> GetCartAsync(Guid userId);
        Task<CartDto> AddToCartAsync(Guid userId, AddCartItemDto request);
        Task<CartDto> UpdateCartItemAsync(Guid userId, Guid cartItemId, int quantity);
        Task<bool> RemoveFromCartAsync(Guid userId, Guid cartItemId);
        Task<bool> ClearCartAsync(Guid userId);
        Task<int> GetCartItemCountAsync(Guid userId);
    }
}

