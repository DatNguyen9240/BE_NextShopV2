using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class UpdateWelcomeCouponSettingsRequest
    {
        [Range(0.01, 100.00)]
        public decimal DiscountPercent { get; set; } = 10;

        [Range(0.01, double.MaxValue)]
        public decimal? MinOrderAmount { get; set; } = 100000;

        [Range(0.01, double.MaxValue)]
        public decimal? MaxDiscountAmount { get; set; } = 50000;

        [Range(1, int.MaxValue)]
        public int UsageLimit { get; set; } = 1;

        [Range(1, 12)]
        public int ValidityMonths { get; set; } = 1;

        public bool IsEnabled { get; set; } = true;
    }
}