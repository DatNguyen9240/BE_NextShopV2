using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class UpdateCouponRequest
    {
        [StringLength(20, MinimumLength = 3)]
        public string? Code { get; set; }

        [Range(0.01, 100.00)]
        public decimal? DiscountPercent { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool? IsActive { get; set; }
    }
}