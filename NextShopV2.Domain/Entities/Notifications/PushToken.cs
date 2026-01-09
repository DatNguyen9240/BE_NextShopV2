using System;

namespace NextShopV2.Domain.Entities.Notifications
{
    public class PushToken
    {
        public Guid PushTokenId { get; set; } = Guid.NewGuid();

        // Optional user association (null for guests)
        public Guid? UserId { get; set; }

        // The actual FCM device token or web push token
        public string Token { get; set; } = string.Empty;

        // Platform: "web", "android", "ios" etc.
        public string Platform { get; set; } = "web";

        // Optional device identifier (for mobile devices)
        public string? DeviceId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastSeenAt { get; set; }
    }
}