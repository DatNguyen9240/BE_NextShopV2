using NextShopV2.Shared.Models;
using System;
using System.Threading.Tasks;

namespace NextShopV2.Shared.Interfaces
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

