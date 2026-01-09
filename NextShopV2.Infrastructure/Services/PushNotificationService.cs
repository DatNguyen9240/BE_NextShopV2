using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NextShopV2.Shared.Interfaces;
using NextShopV2.Infrastructure.Persistence;
using NextShopV2.Domain.Entities.Notifications;

namespace NextShopV2.Infrastructure.Services
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<PushNotificationService> _logger;
        private readonly IConfiguration _config;

        public PushNotificationService(AppDbContext db, ILogger<PushNotificationService> logger, IConfiguration config)
        {
            _db = db;
            _logger = logger;
            _config = config;

            // Initialize Firebase app if not initialized
            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    var svcJson = _config["Firebase:ServiceAccountJson"];
                    var svcPath = _config["Firebase:ServiceAccountFile"] ?? Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT_FILE");
                    if (!string.IsNullOrEmpty(svcJson))
                    {
                        var cred = GoogleCredential.FromJson(svcJson);
                        FirebaseApp.Create(new AppOptions() { Credential = cred });
                        _logger.LogInformation("FirebaseApp initialized from ServiceAccountJson");
                    }
                    else if (!string.IsNullOrEmpty(svcPath) && System.IO.File.Exists(svcPath))
                    {
                        var cred = GoogleCredential.FromFile(svcPath);
                        FirebaseApp.Create(new AppOptions() { Credential = cred });
                        _logger.LogInformation("FirebaseApp initialized from ServiceAccountFile: {Path}", svcPath);
                    }
                    else
                    {
                        _logger.LogWarning("Firebase ServiceAccountJson or ServiceAccountFile not configured - Push notifications disabled");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize FirebaseApp");
            }
        }

        public async Task RegisterTokenAsync(Guid? userId, string token, string platform, string? deviceId = null)
        {
            if (string.IsNullOrWhiteSpace(token)) return;
            var existing = _db.PushTokens.FirstOrDefault(p => p.Token == token);
            if (existing != null)
            {
                existing.IsActive = true;
                existing.LastSeenAt = DateTime.UtcNow;
                existing.Platform = platform ?? existing.Platform;
                existing.DeviceId = deviceId ?? existing.DeviceId;
                if (userId != null) existing.UserId = userId;
            }
            else
            {
                _db.PushTokens.Add(new PushToken { Token = token, Platform = platform ?? "web", DeviceId = deviceId, UserId = userId, IsActive = true, CreatedAt = DateTime.UtcNow, LastSeenAt = DateTime.UtcNow });
            }
            await _db.SaveChangesAsync();
        }

        public async Task UnregisterTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return;
            var existing = _db.PushTokens.FirstOrDefault(p => p.Token == token);
            if (existing != null)
            {
                existing.IsActive = false;
                await _db.SaveChangesAsync();
            }
        }

        public async Task SendToUserAsync(Guid userId, string title, string body, Dictionary<string, string>? data = null)
        {
            var tokens = _db.PushTokens.Where(p => p.UserId == userId && p.IsActive).Select(p => p.Token).ToList();
            if (tokens == null || tokens.Count == 0)
            {
                _logger.LogInformation("No push tokens found for user {UserId}", userId);
                return;
            }
            await SendToTokensAsync(tokens, title, body, data);
        }

        public async Task SendToTokensAsync(IEnumerable<string> tokens, string title, string body, Dictionary<string, string>? data = null)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                _logger.LogWarning("Firebase is not initialized, skipping sending push notification");
                return;
            }

            var tokenList = tokens.Where(t => !string.IsNullOrEmpty(t)).Distinct().ToList();
            if (tokenList.Count == 0) return;

            try
            {
                // Firebase supports Multicast
                var msg = new MulticastMessage()
                {
                    Tokens = tokenList,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data ?? new Dictionary<string, string>()
                };

                var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(msg);
                _logger.LogInformation("Sent {SuccessCount}/{Total} push notifications", response.SuccessCount, response.SuccessCount + response.FailureCount);

                // Handle failures: deactivate tokens that are invalid
                if (response.FailureCount > 0)
                {
                    for (int i = 0; i < response.Responses.Count; i++)
                    {
                        var resp = response.Responses[i];
                        if (!resp.IsSuccess)
                        {
                            var badToken = tokenList[i];
                            _logger.LogWarning("Token failed: {Token} - {Error}", badToken, resp.Exception?.Message);

                            var existing = _db.PushTokens.FirstOrDefault(p => p.Token == badToken);
                            if (existing != null)
                            {
                                existing.IsActive = false;
                            }
                        }
                    }
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send push notification");
            }
        }
    }
}