using System;

namespace NextShopV2.Domain.Entities.Marketing
{
    public class WelcomeVoucherIssuance
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string IdentifierHash { get; set; } = null!; // hashed email or phone
        public Guid? UserId { get; set; }
        public string? Ip { get; set; }
        public string? VoucherCode { get; set; }
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }
}