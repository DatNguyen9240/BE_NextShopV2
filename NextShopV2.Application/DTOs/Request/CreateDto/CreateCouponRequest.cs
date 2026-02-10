using System.ComponentModel.DataAnnotations;
using NextShopV2.Domain.Entities.Coupons;

namespace NextShopV2.Application.DTOs.Request
{
    public class CreateCouponRequest
    {
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Code { get; set; } = null!;

        public Guid? UserId { get; set; } // Optional: null for global coupons

        public string? CouponType { get; set; } // "Welcome", "Manual", "Promotion", etc.

        [Required]
        [Range(0.01, 100.00)]
        public decimal DiscountPercent { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? MinOrderAmount { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? MaxDiscountAmount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(1, int.MaxValue)]
        public int? UsageLimit { get; set; }

        public bool IsActive { get; set; } = true;
    }
}