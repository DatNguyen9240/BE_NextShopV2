using System;

namespace NextShopV2.Application.DTOs.Response
{
    public class WelcomeCouponSettingsResponse
    {
        public Guid Id { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public int UsageLimit { get; set; }
        public int ValidityMonths { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}