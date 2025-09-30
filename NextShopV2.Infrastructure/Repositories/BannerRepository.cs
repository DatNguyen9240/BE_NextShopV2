using NextShopV2.Application.Interfaces;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Domain.Entities.Marketing;
using NextShopV2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace NextShopV2.Infrastructure.Repositories
{
    public class BannerRepository : IBannerRepository
    {
        private readonly AppDbContext _context;
        public BannerRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<Advertisement>> GetAllAsync()
            => await _context.Advertisements.OrderBy(a => a.SortOrder).ToListAsync();
        public async Task<Advertisement?> GetByIdAsync(Guid id)
            => await _context.Advertisements.FindAsync(id);
        public async Task AddAsync(Advertisement banner)
        {
            await _context.Advertisements.AddAsync(banner);
        }
        public Task UpdateAsync(Advertisement banner)
        {
            _context.Advertisements.Update(banner);
            return Task.CompletedTask;
        }
        public Task DeleteAsync(Advertisement banner)
        {
            _context.Advertisements.Remove(banner);
            return Task.CompletedTask;
        }
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
