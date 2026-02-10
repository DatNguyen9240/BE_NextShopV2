using System;

namespace NextShopV2.Domain.Entities.Coupons
{
    public class WelcomeCouponSettings
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public decimal DiscountPercent { get; set; } = 10; // Default 10%
        public decimal? MinOrderAmount { get; set; } = 100000; // VND
        public decimal? MaxDiscountAmount { get; set; } = 50000; // VND
        public int UsageLimit { get; set; } = 1; // Per user
        public int ValidityMonths { get; set; } = 1; // Months from creation
        public bool IsEnabled { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}