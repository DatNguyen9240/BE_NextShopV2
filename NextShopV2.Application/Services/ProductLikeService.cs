using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Domain.Entities.Interactions;
using NextShopV2.Shared.Extensions;

namespace NextShopV2.Application.Services
{
    public class ProductLikeService : IProductLikeService
    {
        private readonly IProductLikeRepository _productLikeRepo;
        private readonly IProductRepository _productRepo;

        public ProductLikeService(IProductLikeRepository productLikeRepo, IProductRepository productRepo)
        {
            _productLikeRepo = productLikeRepo;
            _productRepo = productRepo;
        }

        public async Task<bool> LikeProductAsync(Guid userId, Guid productId)
        {
            // Check if product exists
            var product = await _productRepo.GetByIdAsync(productId);
            if (product.IsNull())
                throw new ArgumentException("Product not found");

            // Check if already liked
            var existingLike = await _productLikeRepo.GetByUserAndProductAsync(userId, productId);
            if (existingLike != null)
                return false; // Already liked

            var productLike = new ProductLike
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };

            await _productLikeRepo.AddLikeAsync(productLike);

            // Update product total likes count
            product!.TotalLikes = await _productLikeRepo.GetProductLikeCountAsync(productId);
            await _productRepo.UpdateAsync(product);

            return true;
        }

        public async Task<bool> UnlikeProductAsync(Guid userId, Guid productId)
        {
            var result = await _productLikeRepo.RemoveLikeAsync(userId, productId);
            
            if (result)
            {
                // Update product total likes count
                var product = await _productRepo.GetByIdAsync(productId);
                if (!product.IsNull())
                {
                    product!.TotalLikes = await _productLikeRepo.GetProductLikeCountAsync(productId);
                    await _productRepo.UpdateAsync(product);
                }
            }

            return result;
        }

        public async Task<bool> ToggleLikeAsync(Guid userId, Guid productId)
        {
            var isLiked = await _productLikeRepo.IsLikedByUserAsync(userId, productId);
            
            if (isLiked)
            {
                await UnlikeProductAsync(userId, productId);
                return false; // Now unliked
            }
            else
            {
                await LikeProductAsync(userId, productId);
                return true; // Now liked
            }
        }

        public async Task<bool> IsLikedByUserAsync(Guid userId, Guid productId)
        {
            return await _productLikeRepo.IsLikedByUserAsync(userId, productId);
        }

        public async Task<List<ProductLikeResponse>> GetUserLikesAsync(Guid userId)
        {
            var likes = await _productLikeRepo.GetUserLikesAsync(userId);
            return likes.Select(MapToResponse).ToList();
        }

        public async Task<int> GetProductLikeCountAsync(Guid productId)
        {
            return await _productLikeRepo.GetProductLikeCountAsync(productId);
        }

        public async Task<List<Guid>> GetLikedProductIdsAsync(Guid userId)
        {
            var likes = await _productLikeRepo.GetUserLikesAsync(userId);
            return likes.Select(l => l.ProductId).ToList();
        }

        private ProductLikeResponse MapToResponse(ProductLike productLike)
        {
            return new ProductLikeResponse
            {
                ProductId = productLike.ProductId,
                UserId = productLike.UserId,
                CreatedAt = productLike.CreatedAt,
                ProductName = productLike.Product?.Name ?? ""
            };
        }
    }
}