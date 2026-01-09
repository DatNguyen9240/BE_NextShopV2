using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System.Text.Json;
using NextShopV2.Api.Hubs;
using NextShopV2.Api.Models;

namespace NextShopV2.Api.Services
{
    public class NotificationSubscriberHostedService : BackgroundService
    {
        private readonly IConnectionMultiplexer _multiplexer;
        private readonly IHubContext<NotificationHub> _hubContext;
        private const string Channel = "notifications:published";

        public NotificationSubscriberHostedService(IConnectionMultiplexer multiplexer, IHubContext<NotificationHub> hubContext)
        {
            _multiplexer = multiplexer;
            _hubContext = hubContext;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var sub = _multiplexer.GetSubscriber();
            // Subscribe (keeps subscription even if multiple instances)
            sub.Subscribe(RedisChannel.Literal(Channel), async (channel, message) =>
            {
                try
                {
                    var msg = message.HasValue ? message.ToString() : null;
                    Console.WriteLine($"NotificationSubscriber received message on {channel}: {msg}");
                    if (string.IsNullOrEmpty(msg)) return;
                    var doc = JsonSerializer.Deserialize<JsonElement>(msg);
                    if (doc.ValueKind != JsonValueKind.Object) return;
                    if (!doc.TryGetProperty("userId", out var userIdProp)) return;
                    if (!doc.TryGetProperty("notification", out var notifProp)) return;

                    var userId = userIdProp.GetString();
                    var notifJson = notifProp.GetRawText();
                    var notification = JsonSerializer.Deserialize<NotificationDto>(notifJson, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    Console.WriteLine($"NotificationSubscriber: parsed notification for user '{userId}': {notifJson}");
                    if (string.IsNullOrEmpty(userId) || notification == null)
                    {
                        Console.WriteLine("NotificationSubscriber: invalid payload (missing userId or notification)");
                        return;
                    }

                    Console.WriteLine($"NotificationSubscriber: sending notification to user '{userId}': {notifJson}");
                    try
                    {
                        if (userId == "*")
                        {
                            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
                            Console.WriteLine("NotificationSubscriber: sent notification to ALL clients");
                        }
                        else
                        {
                            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notification);
                            Console.WriteLine($"NotificationSubscriber: sent notification to user '{userId}'");
                        }
                    }
                    catch (Exception exSend)
                    {
                        Console.WriteLine($"NotificationSubscriber: failed to send to Hub for user '{userId}': {exSend.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"NotificationSubscriber: error processing message: {ex.Message}");
                }
            });

            return Task.CompletedTask;
        }
    }
}