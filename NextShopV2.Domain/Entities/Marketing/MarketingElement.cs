using System;

namespace NextShopV2.Domain.Entities.Marketing
{
    public class MarketingElement
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "Data";
        public string ClassName { get; set; } = "w-full bg-purple-600 text-white text-center py-1 px-2 text-sm font-semibold";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}