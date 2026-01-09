using Microsoft.AspNetCore.Mvc;
using NextShopV2.Shared.Interfaces;
using NextShopV2.Api.Attributes;
using NextShopV2.Shared.Extensions.Web;
using Microsoft.AspNetCore.Authorization;
using FirebaseAdmin;
using System.Threading.Tasks;
using System;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PushController : ControllerBase
    {
        private readonly IPushNotificationService _pushService;

        public PushController(IPushNotificationService pushService)
        {
            _pushService = pushService;
        }

        public class RegisterRequest { public string Token { get; set; } = ""; public string Platform { get; set; } = "web"; public string? DeviceId { get; set; } }
        public class UnregisterRequest { public string Token { get; set; } = ""; }

        [HttpPost("register")]
        [Authenticated]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var (userId, err) = this.GetCurrentUserId();
            if (err != null) return Unauthorized(err);
            await _pushService.RegisterTokenAsync(userId, req.Token, req.Platform ?? "web", req.DeviceId);
            return Ok(new { success = true });
        }

        [HttpPost("unregister")]
        [Authenticated]
        public async Task<IActionResult> Unregister([FromBody] UnregisterRequest req)
        {
            var (userId, err) = this.GetCurrentUserId();
            if (err != null) return Unauthorized(err);
            await _pushService.UnregisterTokenAsync(req.Token);
            return Ok(new { success = true });
        }

        // Allow guests (unauthenticated) to register a push token. This is useful for web clients
        // that want to receive notifications without forcing a login immediately.
        [HttpPost("register-guest")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterGuest([FromBody] RegisterRequest req)
        {
            await _pushService.RegisterTokenAsync(null, req.Token, req.Platform ?? "web", req.DeviceId);
            return Ok(new { success = true });
        }

        [HttpGet("status")]
        [AllowAnonymous]
        public IActionResult Status()
        {
            bool firebaseInitialized = FirebaseApp.DefaultInstance != null;
            return Ok(new { firebaseInitialized });
        }

        [HttpPost("send-to-user/{userId}")]
        [AdminOnly]
        public async Task<IActionResult> SendToUser(Guid userId, [FromBody] dynamic payload)
        {
            string title = payload?.title ?? "Thông báo";
            string body = payload?.body ?? "Bạn có cập nhật mới";
            await _pushService.SendToUserAsync(userId, title, body, null);
            return Ok(new { success = true });
        }
    }
}