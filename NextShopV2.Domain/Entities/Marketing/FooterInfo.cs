using System;

namespace NextShopV2.Domain.Entities.Marketing
{
    public class FooterInfo
    {
        public Guid Id { get; set; }
        public string ClassName { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}