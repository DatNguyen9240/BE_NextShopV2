using NextShopV2.Application.Interfaces;
using NextShopV2.Domain.Entities.Marketing;
using NextShopV2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace NextShopV2.Infrastructure.Repositories
{
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly AppDbContext _context;

        public AnnouncementRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Announcement?> GetActiveAsync()
        {
            return await _context.Announcements
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.UpdatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Announcement>> GetAllAsync()
        {
            return await _context.Announcements
                .OrderByDescending(a => a.UpdatedAt)
                .ToListAsync();
        }

        public async Task<Announcement?> GetByIdAsync(Guid id)
        {
            return await _context.Announcements.FindAsync(id);
        }

        public async Task AddAsync(Announcement announcement)
        {
            await _context.Announcements.AddAsync(announcement);
        }

        public async Task DeleteAsync(Announcement announcement)
        {
            _context.Announcements.Remove(announcement);
        }

        public async Task UpdateAsync(Announcement announcement)
        {
            _context.Announcements.Update(announcement);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}