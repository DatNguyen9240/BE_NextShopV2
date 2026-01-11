using NextShopV2.Domain.Entities.Notifications;

namespace NextShopV2.Domain.Repositories
{
    public interface INotificationHistoryRepository
    {
        Task<NotificationHistory?> GetByIdAsync(Guid id);
        Task<IEnumerable<NotificationHistory>> GetAllAsync();
        Task<IEnumerable<NotificationHistory>> GetByUserIdAsync(string userId);
        Task AddAsync(NotificationHistory notificationHistory);
        Task UpdateAsync(NotificationHistory notificationHistory);
        Task DeleteAsync(Guid id);
    }
}