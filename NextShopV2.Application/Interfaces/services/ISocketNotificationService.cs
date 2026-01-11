using NextShopV2.Application.DTOs;
using NextShopV2.Application.DTOs.Response;

namespace NextShopV2.Application.Interfaces.Services
{
    public interface ISocketNotificationService
    {
        Task AddNotificationAsync(string userId, SocketNotificationDto notification);
        Task<List<SocketNotificationDto>> GetNotificationsAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkReadAsync(string userId, string notificationId);
        Task MarkAllReadAsync(string userId);
    }
}