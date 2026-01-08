using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class ApplyCouponRequest
    {
        [Required]
        [StringLength(20)]
        public string CouponCode { get; set; } = null!;
    }
}