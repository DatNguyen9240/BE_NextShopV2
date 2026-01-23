using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Application.DTOs;
using NextShopV2.Application.DTOs.Request.CreateDto;
using NextShopV2.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Configuration;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace NextShopV2.Infrastructure.Services
{
    public class FirebaseNotificationService : IFirebaseNotificationService
    {
        private readonly ILogger<FirebaseNotificationService> _logger;
        private readonly FirebaseMessaging _messaging;
        private readonly IConfiguration _configuration;

        public FirebaseNotificationService(ILogger<FirebaseNotificationService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            // Khởi tạo Firebase Admin SDK
            if (FirebaseApp.DefaultInstance == null)
            {
                var firebaseJson = Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT_JSON");
                
                try
                {
                    if (!string.IsNullOrEmpty(firebaseJson))
                    {
                        _logger.LogInformation("Initializing Firebase using environment variable FIREBASE_SERVICE_ACCOUNT_JSON");
                        
                        // Nếu chuỗi không bắt đầu bằng '{', thử giải mã Base64
                        string jsonToUse = firebaseJson.Trim();
                        if (!jsonToUse.StartsWith("{"))
                        {
                            try
                            {
                                var base64Bytes = Convert.FromBase64String(jsonToUse);
                                jsonToUse = System.Text.Encoding.UTF8.GetString(base64Bytes);
                                _logger.LogInformation("Firebase JSON successfully decoded from Base64");
                            }
                            catch (FormatException)
                            {
                                _logger.LogWarning("FIREBASE_SERVICE_ACCOUNT_JSON does not start with '{' and is not a valid Base64 string.");
                            }
                        }

                        FirebaseApp.Create(new AppOptions()
                        {
                            Credential = GoogleCredential.FromJson(jsonToUse)
                        });
                    }
                    else
                    {
                        var keyPath = _configuration["Firebase:ServiceAccountKeyPath"] ?? "Config/serviceAccountKey.json";
                        _logger.LogInformation($"Initializing Firebase with key path: {keyPath}");
                        FirebaseApp.Create(new AppOptions()
                        {
                            Credential = GoogleCredential.FromFile(keyPath)
                        });
                    }
                    _logger.LogInformation("Firebase Admin SDK initialized successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to initialize Firebase: {ex.Message}");
                    throw;
                }
            }

            _messaging = FirebaseMessaging.DefaultInstance;
            _logger.LogInformation("Firebase Messaging instance created");
        }

        public async Task<string> SendNotificationAsync(string token, FirebaseNotificationRequest notification)
        {
            try
            {
                if (string.IsNullOrEmpty(notification.Title) || string.IsNullOrEmpty(notification.Body))
                {
                    throw new ArgumentException("Title and Body are required for notifications");
                }

                var message = new Message()
                {
                    Token = token,
                    Notification = new Notification()
                    {
                        Title = notification.Title,
                        Body = notification.Body,
                        ImageUrl = notification.ImageUrl
                    },
                    Data = notification.Data,
                    Android = new AndroidConfig()
                    {
                        Priority = Priority.High
                    }
                };

                string response = await _messaging.SendAsync(message);
                _logger.LogInformation($"Successfully sent message: {response}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending notification: {ex.Message}");
                _logger.LogError($"Exception type: {ex.GetType().Name}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                    _logger.LogError($"Inner exception type: {ex.InnerException.GetType().Name}");
                }
                throw;
            }
        }

        public async Task<FirebaseNotificationSendResult> SendToMultipleAsync(List<string> tokens, FirebaseNotificationRequest notification)
        {
            try
            {
                if (string.IsNullOrEmpty(notification.Title) || string.IsNullOrEmpty(notification.Body))
                {
                    throw new ArgumentException("Title and Body are required for notifications");
                }

                var message = new MulticastMessage()
                {
                    Tokens = tokens,
                    Notification = new Notification()
                    {
                        Title = notification.Title,
                        Body = notification.Body,
                        ImageUrl = notification.ImageUrl
                    },
                    Data = notification.Data,
                    Android = new AndroidConfig()
                    {
                        Priority = Priority.High
                    }
                };

                var response = await _messaging.SendEachForMulticastAsync(message);
                _logger.LogInformation($"Successfully sent {response.SuccessCount} messages, {response.FailureCount} failures");
                
                var result = new FirebaseNotificationSendResult
                {
                    SuccessCount = response.SuccessCount,
                    FailureCount = response.FailureCount,
                    Summary = $"Success: {response.SuccessCount}, Failure: {response.FailureCount}"
                };

                if (response.FailureCount > 0 && response.Responses != null)
                {
                    for (int i = 0; i < response.Responses.Count; i++)
                    {
                        if (!response.Responses[i].IsSuccess)
                        {
                            var exceptionMessage = response.Responses[i].Exception?.Message ?? "Unknown error";
                            _logger.LogError($"Failed to send to token {i}: {exceptionMessage}");
                            
                            // Check if token is not registered
                            if (exceptionMessage.Contains("NotRegistered") || exceptionMessage.Contains("not registered"))
                            {
                                result.InvalidTokens.Add(tokens[i]);
                            }
                        }
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending notifications: {ex.Message}");
                _logger.LogError($"Exception details: {ex.ToString()}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<string> SendToTopicAsync(string topic, FirebaseNotificationRequest notification)
        {
            try
            {
                if (string.IsNullOrEmpty(notification.Title) || string.IsNullOrEmpty(notification.Body))
                {
                    throw new ArgumentException("Title and Body are required for notifications");
                }

                var message = new Message()
                {
                    Topic = topic,
                    Notification = new Notification()
                    {
                        Title = notification.Title,
                        Body = notification.Body,
                        ImageUrl = notification.ImageUrl
                    },
                    Data = notification.Data
                };

                string response = await _messaging.SendAsync(message);
                _logger.LogInformation($"Successfully sent message to topic: {response}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending notification to topic: {ex.Message}");
                throw;
            }
        }
    }
}