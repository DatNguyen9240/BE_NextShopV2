using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Domain.Entities.Interactions;
using NextShopV2.Shared.Extensions;

namespace NextShopV2.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly IProductRepository _productRepo;
        private readonly IOrderRepository _orderRepo;

        public ReviewService(IReviewRepository reviewRepo, IProductRepository productRepo, IOrderRepository orderRepo)
        {
            _reviewRepo = reviewRepo;
            _productRepo = productRepo;
            _orderRepo = orderRepo;
        }

        public async Task<List<ReviewResponse>> GetAllAsync()
        {
            var reviews = await _reviewRepo.GetAllAsync();
            return reviews.Select(MapToResponse).ToList();
        }

        public async Task<ReviewResponse?> GetByIdAsync(Guid id)
        {
            var review = await _reviewRepo.GetByIdAsync(id);
            return review.IsNull() ? null : MapToResponse(review!);
        }

        public async Task<List<ReviewResponse>> GetByProductIdAsync(Guid productId)
        {
            var reviews = await _reviewRepo.GetByProductIdAsync(productId);
            return reviews.Select(MapToResponse).ToList();
        }

        public async Task<List<ReviewResponse>> GetByUserIdAsync(Guid userId)
        {
            var reviews = await _reviewRepo.GetByUserIdAsync(userId);
            return reviews.Select(MapToResponse).ToList();
        }

        public async Task<ReviewResponse> CreateAsync(Guid userId, CreateReviewRequest request)
        {
            // Check if product exists
            var product = await _productRepo.GetByIdAsync(request.ProductId);
            if (product.IsNull())
                throw new ArgumentException("Product not found");

            // Check if user already reviewed this product
            var existingReview = await _reviewRepo.GetByUserAndProductAsync(userId, request.ProductId);
            if (existingReview != null)
                throw new ArgumentException("User has already reviewed this product");

            var review = new Review
            {
                ReviewId = Guid.NewGuid(),
                ProductId = request.ProductId,
                UserId = userId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepo.CreateAsync(review);

            // Update product average rating and review count
            await UpdateProductStats(request.ProductId);

            // Reload review with navigation properties
            var createdReview = await _reviewRepo.GetByIdAsync(review.ReviewId);
            return MapToResponse(createdReview!);
        }

        public async Task<ReviewResponse?> UpdateAsync(Guid reviewId, UpdateReviewRequest request)
        {
            var review = await _reviewRepo.GetByIdAsync(reviewId);
            if (review.IsNull())
                return null;

            if (request.Rating.HasValue)
                review!.Rating = request.Rating.Value;

            if (!string.IsNullOrEmpty(request.Comment))
                review!.Comment = request.Comment;

            await _reviewRepo.UpdateAsync(review!);

            // Update product average rating
            await UpdateProductStats(review!.ProductId);

            return MapToResponse(review!);
        }

        public async Task<bool> DeleteAsync(Guid reviewId)
        {
            var review = await _reviewRepo.GetByIdAsync(reviewId);
            if (review.IsNull())
                return false;

            var productId = review!.ProductId;
            var result = await _reviewRepo.DeleteAsync(reviewId);

            if (result)
            {
                // Update product stats after deletion
                await UpdateProductStats(productId);
            }

            return result;
        }

        public async Task<bool> CanUserReviewProductAsync(Guid userId, Guid productId)
        {
            // Check if product exists
            var product = await _productRepo.GetByIdAsync(productId);
            if (product.IsNull())
                return false;

            // Check if user hasn't already reviewed this product
            if (await _reviewRepo.HasUserReviewedProductAsync(userId, productId))
                return false;

            // Check if user has purchased this product (order status Completed)
            var orders = await _orderRepo.GetByUserIdAsync(userId);
            var hasPurchased = orders.Any(o => o.Status == "Completed" && 
                                               o.Items.Any(oi => oi.ProductId == productId));
            return hasPurchased;
        }

        public async Task<object> GetProductReviewStatsAsync(Guid productId)
        {
            var averageRating = await _reviewRepo.GetAverageRatingAsync(productId);
            var reviewCount = await _reviewRepo.GetReviewCountAsync(productId);

            return new
            {
                AverageRating = averageRating,
                ReviewCount = reviewCount
            };
        }

        private async Task UpdateProductStats(Guid productId)
        {
            var product = await _productRepo.GetByIdAsync(productId);
            if (!product.IsNull())
            {
                product!.AverageRating = (decimal)await _reviewRepo.GetAverageRatingAsync(productId);
                product.TotalReviews = await _reviewRepo.GetReviewCountAsync(productId);
                await _productRepo.UpdateAsync(product);
            }
        }

        private ReviewResponse MapToResponse(Review review)
        {
            return new ReviewResponse
            {
                ReviewId = review.ReviewId,
                ProductId = review.ProductId,
                UserId = review.UserId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                ProductName = review.Product?.Name ?? "",
                UserName = review.User?.FullName ?? ""
            };
        }
    }
}