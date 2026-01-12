using NextShopV2.Shared.Interfaces;
using NextShopV2.Application.DTOs;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Application.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;
using NextShopV2.Api.Hubs;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace NextShopV2.Api.Services
{
    public class SocketNotificationService : ISocketNotificationService
    {
        private readonly IHubContext<SocketNotificationHub> _hubContext;
        private readonly IDistributedCache _cache;

        public SocketNotificationService(IHubContext<SocketNotificationHub> hubContext, IDistributedCache cache)
        {
            _hubContext = hubContext;
            _cache = cache;
        }

        public async Task AddNotificationAsync(string userId, SocketNotificationDto notification)
        {
            var notifications = await GetNotificationsAsync(userId);
            notifications.Insert(0, notification); // Add to top

            // Keep only last 100 notifications
            if (notifications.Count > 100)
            {
                notifications = notifications.Take(100).ToList();
            }

            await SaveNotificationsAsync(userId, notifications);

            // Send to connected clients
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notification);
        }

        public async Task<List<SocketNotificationDto>> GetNotificationsAsync(string userId)
        {
            var cacheKey = $"notifications:{userId}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                return JsonSerializer.Deserialize<List<SocketNotificationDto>>(cached) ?? new List<SocketNotificationDto>();
            }
            return new List<SocketNotificationDto>();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            var notifications = await GetNotificationsAsync(userId);
            return notifications.Count(n => !n.Read);
        }

        public async Task MarkReadAsync(string userId, string notificationId)
        {
            var notifications = await GetNotificationsAsync(userId);
            var notification = notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null)
            {
                notification.Read = true;
                await SaveNotificationsAsync(userId, notifications);
            }
        }

        public async Task MarkAllReadAsync(string userId)
        {
            var notifications = await GetNotificationsAsync(userId);
            foreach (var n in notifications)
            {
                n.Read = true;
            }
            await SaveNotificationsAsync(userId, notifications);
        }

        private async Task SaveNotificationsAsync(string userId, List<SocketNotificationDto> notifications)
        {
            var cacheKey = $"notifications:{userId}";
            var json = JsonSerializer.Serialize(notifications);
            await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30) // Expire after 30 days
            });
        }
    }
}