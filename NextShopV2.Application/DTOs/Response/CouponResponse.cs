namespace NextShopV2.Application.DTOs.Response
{
    public class CouponResponse
    {
        public Guid CouponId { get; set; }
        public string Code { get; set; } = null!;
        public decimal DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired => DateTime.UtcNow > EndDate;
        public bool IsValid => IsActive && !IsExpired && DateTime.UtcNow >= StartDate;
    }
}