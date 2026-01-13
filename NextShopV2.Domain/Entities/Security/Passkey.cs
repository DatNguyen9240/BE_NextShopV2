using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NextShopV2.Domain.Entities.Security
{
    public class Passkey
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public Guid UserId { get; set; }

        // Stored as base64url string
        public string CredentialId { get; set; } = string.Empty;

        // Stored as base64 string
        public string PublicKey { get; set; } = string.Empty;

        public long Counter { get; set; } = 0;

        public string? Transports { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastUsedAt { get; set; }

        [ForeignKey("UserId")]
        public NextShopV2.Domain.Entities.Users.User? User { get; set; }
    }
}