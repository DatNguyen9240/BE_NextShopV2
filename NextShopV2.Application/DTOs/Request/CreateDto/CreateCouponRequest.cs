using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class CreateCouponRequest
    {
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Code { get; set; } = null!;

        [Required]
        [Range(0.01, 100.00)]
        public decimal DiscountPercent { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}