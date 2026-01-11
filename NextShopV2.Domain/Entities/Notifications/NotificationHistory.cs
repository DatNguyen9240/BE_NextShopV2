using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NextShopV2.Domain.Entities.Notifications
{
    public class NotificationHistory
    {
        [Key]
        public Guid NotificationHistoryId { get; set; } = Guid.NewGuid();

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public string? TargetToken { get; set; } // Specific token or null for broadcast

        public string? UserId { get; set; } // Associated user if applicable

        public bool IsSuccessful { get; set; }

        public string? ErrorMessage { get; set; }

        public string? FirebaseResponse { get; set; } // JSON response from Firebase

        public string? Data { get; set; } // JSON serialized data

        public int RecipientCount { get; set; }

        public string? Status { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SentAt { get; set; }
    }
}