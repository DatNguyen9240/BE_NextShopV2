using NextShopV2.Api.Models;

namespace NextShopV2.Api.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetNotificationsAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task AddNotificationAsync(string userId, NotificationDto notification);
        Task MarkReadAsync(string userId, string id);
        Task MarkAllReadAsync(string userId);
    }
}