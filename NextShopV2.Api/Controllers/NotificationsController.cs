using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NextShopV2.Api.Hubs;
using NextShopV2.Api.Models;
using NextShopV2.Api.Services;
using System.Collections.Concurrent;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;

        public NotificationsController(IHubContext<NotificationHub> hubContext, INotificationService notificationService)
        {
            _hubContext = hubContext;
            _notificationService = notificationService;
        }

        private string GetUserId()
        {
            // assumes JWT has NameIdentifier claim
            return User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        }

        [HttpGet]
        public async Task<ActionResult> GetMyNotifications()
        {
            var userId = GetUserId();
            var items = (await _notificationService.GetNotificationsAsync(userId)).OrderByDescending(n => n.CreatedAt).ToList();
            var total = items.Count;
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { items, total, unreadCount });
        }

        [HttpPost("send")]
        [AllowAnonymous] // keep for testing; in prod protect this endpoint
        public async Task<ActionResult> Send([FromBody] NotificationDto payload, [FromQuery] string? userId = null)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await _notificationService.AddNotificationAsync(userId, payload);
            }
            else
            {
                // broadcast: iterate over connected keys is not required; you could maintain a list of known users
                // For demo, publish to a special broadcast channel and clients can handle or server-side subscribe to broadcast
                await _notificationService.AddNotificationAsync("*", payload);
            }

            return Ok(payload);
        }

        [HttpPost("{id}/read")]
        public async Task<ActionResult> MarkRead(string id)
        {
            var userId = GetUserId();
            await _notificationService.MarkReadAsync(userId, id);
            return NoContent();
        }

        [HttpPost("mark-all-read")]
        public async Task<ActionResult> MarkAllRead()
        {
            var userId = GetUserId();
            await _notificationService.MarkAllReadAsync(userId);
            return NoContent();
        }

        // helper endpoint to seed a sample notification for the current user
        [HttpPost("seed-my-notif")]
        public async Task<ActionResult> SeedMyNotif()
        {
            var userId = GetUserId();
            var n = new NotificationDto
            {
                Title = "Sample notification",
                Body = "This is a seeded notification",
                Url = "/",
                Read = false
            };
            await _notificationService.AddNotificationAsync(userId, n);
            return Ok(n);
        }
    }
}
