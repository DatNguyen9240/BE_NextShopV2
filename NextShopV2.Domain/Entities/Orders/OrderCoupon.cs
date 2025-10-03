using System;
using NextShopV2.Domain.Entities.Coupons;

namespace NextShopV2.Domain.Entities.Orders
{
    public class OrderCoupon
    {
        public Guid OrderId { get; set; }
        public Guid CouponId { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Order Order { get; set; } = null!;
        public Coupon Coupon { get; set; } = null!;
    }
}
