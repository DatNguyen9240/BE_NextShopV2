using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs;
using NextShopV2.Application.DTOs.Request.CreateDto;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/firebase-notifications")]
    public class FirebaseNotificationController : ControllerBase
    {
        private readonly IPushNotificationService _notificationService;
        private readonly ILogger<FirebaseNotificationController> _logger;
        private static DateTime _lastTestTime = DateTime.MinValue;
        private const int TEST_RATE_LIMIT_SECONDS = 10;

        public FirebaseNotificationController(
            IPushNotificationService notificationService,
            ILogger<FirebaseNotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpPost("save-token")]
        public async Task<IActionResult> SaveToken([FromBody] FirebaseFcmTokenModel model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.Token))
                {
                    // Lấy userId từ request body, nếu không có thì từ JWT claims
                    var userId = model.UserId ??
                               (User.FindFirst("userId")?.Value ??
                                User.FindFirst("sub")?.Value ??
                                User.Identity?.Name);

                    await _notificationService.SaveTokenAsync(model, userId);

                    _logger.LogInformation($"Token saved to database: {model.Token}");
                    return Ok(new { message = "Token saved successfully to database" });
                }

                return BadRequest("Invalid token");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving token: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Body))
                {
                    return BadRequest("Token, Title, and Body are required");
                }

                var notification = new FirebaseNotificationRequest
                {
                    Title = request.Title,
                    Body = request.Body,
                    ImageUrl = request.ImageUrl,
                    Data = request.Data
                };

                var result = await _notificationService.SendNotificationAsync(request.Token, notification);

                return Ok(new { message = "Notification sent", result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("send-all")]
        public async Task<IActionResult> SendToAll([FromBody] FirebaseNotificationRequest notification)
        {
            try
            {
                if (string.IsNullOrEmpty(notification.Title) || string.IsNullOrEmpty(notification.Body))
                {
                    return BadRequest("Title and Body are required");
                }

                var result = await _notificationService.SendToAllAsync(notification);

                return Ok(new { message = "Notifications sent", result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetNotificationHistory()
        {
            try
            {
                var history = await _notificationService.GetHistoryAsync();
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting notification history: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("tokens")]
        public async Task<IActionResult> GetFcmTokens()
        {
            try
            {
                var tokens = await _notificationService.GetTokensAsync();
                return Ok(tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting FCM tokens: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("test-firebase")]
        public async Task<IActionResult> TestFirebase()
        {
            try
            {
                // Rate limiting: 10 seconds between test calls
                var timeSinceLastTest = DateTime.UtcNow - _lastTestTime;
                if (timeSinceLastTest.TotalSeconds < TEST_RATE_LIMIT_SECONDS)
                {
                    var remainingSeconds = TEST_RATE_LIMIT_SECONDS - (int)timeSinceLastTest.TotalSeconds;
                    _logger.LogWarning($"Test notification rate limited. Try again in {remainingSeconds} seconds.");
                    return BadRequest(new { 
                        message = $"Rate limited. Please wait {remainingSeconds} seconds before testing again.",
                        rateLimited = true,
                        retryAfter = remainingSeconds
                    });
                }

                _lastTestTime = DateTime.UtcNow;
                await _notificationService.TestFirebaseAsync();
                return Ok(new { message = "Test notification sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending test notification: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("clear-tokens")]
        public async Task<IActionResult> ClearTokens()
        {
            try
            {
                await _notificationService.ClearTokensAsync();
                _logger.LogInformation("All FCM tokens cleared from database");
                return Ok(new { message = "All tokens cleared successfully from database" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error clearing tokens: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("remove-token")]
        public async Task<IActionResult> RemoveToken([FromBody] FirebaseFcmTokenModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model?.Token)) return BadRequest("Token is required");
                await _notificationService.RemoveTokenAsync(model.Token);
                _logger.LogInformation($"Token removed: {model.Token}");
                return Ok(new { message = "Token removed" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error removing token: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}