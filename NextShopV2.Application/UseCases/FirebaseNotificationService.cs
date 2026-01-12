using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Domain.Repositories;
using NextShopV2.Application.DTOs;
using NextShopV2.Application.DTOs.Request.CreateDto;
using NextShopV2.Domain.Entities.Notifications;
using NextShopV2.Application.Interfaces.Services;
using System.Text.Json;

namespace NextShopV2.Application.UseCases
{
    public class FirebaseNotificationService : IPushNotificationService
    {
        private readonly IFirebaseNotificationService _firebaseService;
        private readonly IFirebasePushTokenRepository _pushTokenRepository;
        private readonly INotificationHistoryRepository _notificationHistoryRepository;

        public FirebaseNotificationService(
            IFirebaseNotificationService firebaseService,
            IFirebasePushTokenRepository pushTokenRepository,
            INotificationHistoryRepository notificationHistoryRepository
            )
        {
            _firebaseService = firebaseService;
            _pushTokenRepository = pushTokenRepository;
            _notificationHistoryRepository = notificationHistoryRepository;
        }

        public async Task SaveTokenAsync(FirebaseFcmTokenModel model, string? userId)
        {
            // Check if token exists
            var existingToken = await _pushTokenRepository.GetByTokenAsync(model.Token);
            if (existingToken != null)
            {
                // Update LastSeenAt
                existingToken.LastSeenAt = DateTime.UtcNow;
                await _pushTokenRepository.UpdateAsync(existingToken);
                return;
            }

            Guid? parsedUserId = null;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var guid))
            {
                parsedUserId = guid;
            }

            var pushToken = new PushToken
            {
                Token = model.Token,
                UserId = parsedUserId,
                Platform = "web", // Default
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastSeenAt = DateTime.UtcNow
            };

            await _pushTokenRepository.AddAsync(pushToken);
        }

        public async Task<string> SendNotificationAsync(string token, FirebaseNotificationRequest request)
        {
            var result = await _firebaseService.SendNotificationAsync(token, request);

            // Save history
            var history = new NotificationHistory
            {
                Title = request.Title,
                Body = request.Body,
                ImageUrl = request.ImageUrl,
                Data = request.Data != null ? JsonSerializer.Serialize(request.Data) : null,
                RecipientCount = 1,
                Status = "success",
                SentAt = DateTime.UtcNow
            };
            await _notificationHistoryRepository.AddAsync(history);

            return result;
        }

        public async Task<FirebaseNotificationSendResult> SendToAllAsync(FirebaseNotificationRequest request)
        {
            var activeTokens = await _pushTokenRepository.GetActiveTokensAsync();
            var tokenStrings = activeTokens.Select(t => t.Token).ToList();

            if (tokenStrings.Count == 0)
            {
                throw new InvalidOperationException("No FCM tokens available");
            }

            var result = await _firebaseService.SendToMultipleAsync(tokenStrings, request);

            // Disable invalid tokens
            if (result.InvalidTokens.Any())
            {
                foreach (var invalidToken in result.InvalidTokens)
                {
                    var tokenEntity = activeTokens.FirstOrDefault(t => t.Token == invalidToken);
                    if (tokenEntity != null)
                    {
                        tokenEntity.IsActive = false;
                        await _pushTokenRepository.UpdateAsync(tokenEntity);
                    }
                }
            }

            // Save history
            var history = new NotificationHistory
            {
                Title = request.Title,
                Body = request.Body,
                ImageUrl = request.ImageUrl,
                Data = request.Data != null ? JsonSerializer.Serialize(request.Data) : null,
                RecipientCount = tokenStrings.Count,
                Status = "success",
                SentAt = DateTime.UtcNow
            };
            await _notificationHistoryRepository.AddAsync(history);

            return result;
        }

        public async Task<IEnumerable<FirebaseFcmTokenDto>> GetTokensAsync()
        {
            var tokens = await _pushTokenRepository.GetActiveTokensAsync();
            return tokens.Select(t => new FirebaseFcmTokenDto
            {
                Token = t.Token,
                UserId = t.UserId?.ToString(),
                CreatedAt = t.CreatedAt
            });
        }

        public async Task<IEnumerable<FirebaseNotificationHistoryDto>> GetHistoryAsync(int count = 50)
        {
            var histories = await _notificationHistoryRepository.GetAllAsync();
            return histories
                .OrderByDescending(h => h.CreatedAt)
                .Take(count)
                .Select(h => new FirebaseNotificationHistoryDto
                {
                    Id = h.NotificationHistoryId,
                    Title = h.Title,
                    Body = h.Body,
                    ImageUrl = h.ImageUrl,
                    Data = h.Data,
                    RecipientCount = h.RecipientCount,
                    Status = h.Status ?? "unknown",
                    ErrorMessage = h.ErrorMessage,
                    SentAt = h.SentAt ?? h.CreatedAt
                });
        }

        public async Task TestFirebaseAsync()
        {
            var activeTokens = await _pushTokenRepository.GetActiveTokensAsync();
            var tokenStrings = activeTokens.Select(t => t.Token).ToList();

            if (tokenStrings.Count == 0)
            {
                throw new InvalidOperationException("No FCM tokens available");
            }

            var testNotification = new FirebaseNotificationRequest
            {
                Title = "Firebase Test",
                Body = "This is a test notification from NextShop API",
                Data = new Dictionary<string, string> { { "test", "true" } }
            };

            var result = await _firebaseService.SendToMultipleAsync(tokenStrings, testNotification);

            // Disable invalid tokens
            if (result.InvalidTokens.Any())
            {
                foreach (var invalidToken in result.InvalidTokens)
                {
                    var tokenEntity = activeTokens.FirstOrDefault(t => t.Token == invalidToken);
                    if (tokenEntity != null)
                    {
                        tokenEntity.IsActive = false;
                        await _pushTokenRepository.UpdateAsync(tokenEntity);
                    }
                }
            }

            // Save history
            var testHistory = new NotificationHistory
            {
                Title = testNotification.Title,
                Body = testNotification.Body,
                Data = JsonSerializer.Serialize(testNotification.Data),
                RecipientCount = tokenStrings.Count,
                Status = result.FailureCount > 0 ? "partial" : "success",
                ErrorMessage = result.FailureCount > 0 ? $"Failed to send to {result.FailureCount} tokens" : null,
                SentAt = DateTime.UtcNow
            };
            await _notificationHistoryRepository.AddAsync(testHistory);
        }

        public async Task ClearTokensAsync()
        {
            var activeTokens = await _pushTokenRepository.GetActiveTokensAsync();
            foreach (var token in activeTokens)
            {
                token.IsActive = false;
                await _pushTokenRepository.UpdateAsync(token);
            }
        }
    }
}