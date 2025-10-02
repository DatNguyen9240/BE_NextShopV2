using NextShopV2.Domain.Entities.Interactions;

namespace NextShopV2.Application.Interfaces.Repositories
{
    public interface IProductLikeRepository
    {
        Task<ProductLike?> GetByUserAndProductAsync(Guid userId, Guid productId);
        Task<IEnumerable<ProductLike>> GetUserLikesAsync(Guid userId);
        Task<IEnumerable<ProductLike>> GetProductLikesAsync(Guid productId);
        Task<int> GetProductLikeCountAsync(Guid productId);
        Task<ProductLike> AddLikeAsync(ProductLike productLike);
        Task<bool> RemoveLikeAsync(Guid userId, Guid productId);
        Task<bool> IsLikedByUserAsync(Guid userId, Guid productId);
        Task SaveAsync();
    }
}