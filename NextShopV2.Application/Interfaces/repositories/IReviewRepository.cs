using NextShopV2.Domain.Entities.Interactions;

namespace NextShopV2.Application.Interfaces.Repositories
{
    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetAllAsync();
        Task<Review?> GetByIdAsync(Guid id);
        Task<IEnumerable<Review>> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<Review>> GetByUserIdAsync(Guid userId);
        Task<Review?> GetByUserAndProductAsync(Guid userId, Guid productId);
        Task<Review> CreateAsync(Review review);
        Task<Review> UpdateAsync(Review review);
        Task<bool> DeleteAsync(Guid id);
        Task<double> GetAverageRatingAsync(Guid productId);
        Task<int> GetReviewCountAsync(Guid productId);
        Task<bool> HasUserReviewedProductAsync(Guid userId, Guid productId);
        Task SaveAsync();
    }
}