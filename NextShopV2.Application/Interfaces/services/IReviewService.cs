using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;

namespace NextShopV2.Application.Interfaces.Services
{
    public interface IReviewService
    {
        Task<List<ReviewResponse>> GetAllAsync();
        Task<ReviewResponse?> GetByIdAsync(Guid id);
        Task<List<ReviewResponse>> GetByProductIdAsync(Guid productId);
        Task<List<ReviewResponse>> GetByUserIdAsync(Guid userId);
        Task<ReviewResponse> CreateAsync(Guid userId, CreateReviewRequest request);
        Task<ReviewResponse?> UpdateAsync(Guid reviewId, UpdateReviewRequest request);
        Task<bool> DeleteAsync(Guid reviewId);
        Task<bool> CanUserReviewProductAsync(Guid userId, Guid productId);
        Task<object> GetProductReviewStatsAsync(Guid productId);
    }
}