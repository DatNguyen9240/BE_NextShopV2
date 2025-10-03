using System;

namespace NextShopV2.Application.DTOs.Response
{
    public class OrderCouponResponse
    {
        public Guid CouponId { get; set; }
        public string Code { get; set; } = null!;
        public decimal DiscountAmount { get; set; }
        public DateTime AppliedAt { get; set; }
    }
}
