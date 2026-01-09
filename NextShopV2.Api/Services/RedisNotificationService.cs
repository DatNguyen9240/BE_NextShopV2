using System.Text.Json;
using StackExchange.Redis;
using NextShopV2.Api.Models;

namespace NextShopV2.Api.Services
{
    public class RedisNotificationService : INotificationService
    {
        private readonly IConnectionMultiplexer _multiplexer;
        private readonly IDatabase _db;
        private const string Channel = "notifications:published";

        public RedisNotificationService(IConnectionMultiplexer multiplexer)
        {
            _multiplexer = multiplexer;
            _db = multiplexer.GetDatabase();
        }

        private string KeyHash(string userId) => $"notifications:{userId}:hash";
        private string KeySortedSet(string userId) => $"notifications:{userId}:zset";
        private string KeyUnreadSet(string userId) => $"notifications:{userId}:unread";

        public async Task AddNotificationAsync(string userId, NotificationDto notification)
        {
            var id = notification.Id ?? Guid.NewGuid().ToString();
            notification.Id = id;
            var json = JsonSerializer.Serialize(notification);

            // store hash
            await _db.HashSetAsync(KeyHash(userId), id, json);
            // score by ticks
            var score = new DateTimeOffset(notification.CreatedAt).ToUnixTimeMilliseconds();
            await _db.SortedSetAddAsync(KeySortedSet(userId), id, score);
            if (!notification.Read)
            {
                await _db.SetAddAsync(KeyUnreadSet(userId), id);
            }

            // publish for cross-instance delivery
            var payload = JsonSerializer.Serialize(new { userId, notification });
            var sub = _multiplexer.GetSubscriber();
            await sub.PublishAsync(RedisChannel.Literal(Channel), payload);
            Console.WriteLine($"Published redis notification for user {userId}: {payload}");
        }

        public async Task<IEnumerable<NotificationDto>> GetNotificationsAsync(string userId)
        {
            // Merge global (*) notifications and user-specific ones
            var userIds = new List<string> { "*", userId };
            var got = new List<NotificationDto>();

            foreach (var idOwner in userIds)
            {
                var ids = await _db.SortedSetRangeByScoreAsync(KeySortedSet(idOwner), order: Order.Descending);
                if (ids.Length == 0) continue;
                var values = await _db.HashGetAsync(KeyHash(idOwner), ids.Select(x => (RedisValue)x).ToArray());
                foreach (var v in values)
                {
                    if (v.IsNullOrEmpty) continue;
                    try
                    {
                        var s = v.ToString();
                        if (string.IsNullOrEmpty(s)) continue;
                        var n = JsonSerializer.Deserialize<NotificationDto>(s, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (n != null) got.Add(n);
                    }
                    catch { }
                }
            }

            // sort by CreatedAt desc
            return got.OrderByDescending(x => x.CreatedAt);
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            // include global (*) unread set as well
            var a = await _db.SetLengthAsync(KeyUnreadSet(userId));
            var b = await _db.SetLengthAsync(KeyUnreadSet("*"));
            return (int)(a + b);
        }

        public async Task MarkReadAsync(string userId, string id)
        {
            var v = await _db.HashGetAsync(KeyHash(userId), id);
            if (v.IsNullOrEmpty) return;
            var s = v.ToString();
            if (string.IsNullOrEmpty(s)) return;
            try
            {
                var n = JsonSerializer.Deserialize<NotificationDto>(s, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (n == null) return;
                n.Read = true;
                var updated = JsonSerializer.Serialize(n);
                await _db.HashSetAsync(KeyHash(userId), id, updated);
                await _db.SetRemoveAsync(KeyUnreadSet(userId), id);
            }
            catch { }
        }

        public async Task MarkAllReadAsync(string userId)
        {
            var unreadIds = await _db.SetMembersAsync(KeyUnreadSet(userId));
            if (unreadIds.Length == 0) return;
            foreach (var id in unreadIds)
            {
                var v = await _db.HashGetAsync(KeyHash(userId), id);
                if (v.IsNullOrEmpty) continue;
                var s = v.ToString();
                if (string.IsNullOrEmpty(s)) continue;
                try
                {
                    var n = JsonSerializer.Deserialize<NotificationDto>(s, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (n == null) continue;
                    n.Read = true;
                    var updated = JsonSerializer.Serialize(n);
                    await _db.HashSetAsync(KeyHash(userId), id, updated);
                }
                catch { }
            }
            await _db.KeyDeleteAsync(KeyUnreadSet(userId));
        }
    }
}