using NextShopV2.Domain.Repositories;
using NextShopV2.Domain.Entities.Notifications;
using NextShopV2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace NextShopV2.Infrastructure.Repositories
{
    public class NotificationHistoryRepository : INotificationHistoryRepository
    {
        private readonly AppDbContext _context;

        public NotificationHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<NotificationHistory?> GetByIdAsync(Guid id)
        {
            return await _context.NotificationHistories
                .FirstOrDefaultAsync(nh => nh.NotificationHistoryId == id);
        }

        public async Task<IEnumerable<NotificationHistory>> GetAllAsync()
        {
            return await _context.NotificationHistories
                .OrderByDescending(nh => nh.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationHistory>> GetByUserIdAsync(string userId)
        {
            return await _context.NotificationHistories
                .Where(nh => nh.UserId == userId)
                .OrderByDescending(nh => nh.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(NotificationHistory notificationHistory)
        {
            await _context.NotificationHistories.AddAsync(notificationHistory);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(NotificationHistory notificationHistory)
        {
            _context.NotificationHistories.Update(notificationHistory);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var notificationHistory = await GetByIdAsync(id);
            if (notificationHistory != null)
            {
                _context.NotificationHistories.Remove(notificationHistory);
                await _context.SaveChangesAsync();
            }
        }

        // Dashboard metrics
        public async Task<int> GetCountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.NotificationHistories
                .Where(nh => nh.CreatedAt >= startDate && nh.CreatedAt <= endDate)
                .CountAsync();
        }

        public async Task<List<NotificationHistory>> GetRecentAsync(int limit)
        {
            return await _context.NotificationHistories
                .OrderByDescending(nh => nh.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }
    }
}