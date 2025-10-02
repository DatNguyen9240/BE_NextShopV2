using Microsoft.EntityFrameworkCore;
using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Domain.Entities.Interactions;
using NextShopV2.Infrastructure.Persistence;

namespace NextShopV2.Infrastructure.Repositories
{
    public class ProductLikeRepository : IProductLikeRepository
    {
        private readonly AppDbContext _context;

        public ProductLikeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProductLike?> GetByUserAndProductAsync(Guid userId, Guid productId)
        {
            return await _context.ProductLikes
                .Include(pl => pl.Product)
                .Include(pl => pl.User)
                .FirstOrDefaultAsync(pl => pl.UserId == userId && pl.ProductId == productId);
        }

        public async Task<IEnumerable<ProductLike>> GetUserLikesAsync(Guid userId)
        {
            return await _context.ProductLikes
                .Include(pl => pl.Product)
                .Where(pl => pl.UserId == userId)
                .OrderByDescending(pl => pl.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductLike>> GetProductLikesAsync(Guid productId)
        {
            return await _context.ProductLikes
                .Include(pl => pl.User)
                .Where(pl => pl.ProductId == productId)
                .OrderByDescending(pl => pl.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetProductLikeCountAsync(Guid productId)
        {
            return await _context.ProductLikes
                .CountAsync(pl => pl.ProductId == productId);
        }

        public async Task<ProductLike> AddLikeAsync(ProductLike productLike)
        {
            _context.ProductLikes.Add(productLike);
            await SaveAsync();
            return productLike;
        }

        public async Task<bool> RemoveLikeAsync(Guid userId, Guid productId)
        {
            var like = await _context.ProductLikes
                .FirstOrDefaultAsync(pl => pl.UserId == userId && pl.ProductId == productId);
            
            if (like == null)
                return false;

            _context.ProductLikes.Remove(like);
            await SaveAsync();
            return true;
        }

        public async Task<bool> IsLikedByUserAsync(Guid userId, Guid productId)
        {
            return await _context.ProductLikes
                .AnyAsync(pl => pl.UserId == userId && pl.ProductId == productId);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}