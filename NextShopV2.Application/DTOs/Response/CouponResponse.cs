using NextShopV2.Domain.Entities.Coupons;

namespace NextShopV2.Application.DTOs.Response
{
    public class CouponResponse
    {
        public Guid CouponId { get; set; }
        public string Code { get; set; } = null!;
    public decimal DiscountPercent { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public int ReservedCount { get; set; } = 0;
        public bool IsActive { get; set; }
        
        // Computed properties
        public bool IsExpired => DateTime.UtcNow > EndDate;
        public bool IsStarted => DateTime.UtcNow >= StartDate;
        public bool HasUsageLimit => UsageLimit.HasValue;
        public bool IsUsageLimitReached => UsageLimit.HasValue && UsedCount >= UsageLimit.Value;
        public bool IsValid => IsActive && !IsExpired && IsStarted && !IsUsageLimitReached;
    }
}