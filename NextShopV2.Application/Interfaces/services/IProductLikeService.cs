using NextShopV2.Application.DTOs.Response;

namespace NextShopV2.Application.Interfaces.Services
{
    public interface IProductLikeService
    {
        Task<bool> LikeProductAsync(Guid userId, Guid productId);
        Task<bool> UnlikeProductAsync(Guid userId, Guid productId);
        Task<bool> ToggleLikeAsync(Guid userId, Guid productId);
        Task<bool> IsLikedByUserAsync(Guid userId, Guid productId);
        Task<List<ProductLikeResponse>> GetUserLikesAsync(Guid userId);
        Task<int> GetProductLikeCountAsync(Guid productId);
        Task<List<Guid>> GetLikedProductIdsAsync(Guid userId);
    }
}