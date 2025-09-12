using System;
using System.Collections.Generic;
using NextShopV2.Domain.Entities.Orders; // Thêm dòng này

namespace NextShopV2.Domain.Entities.Coupons
{
    public class Coupon
    {
        public Guid CouponId { get; set; }
        public string Code { get; set; } = null!;
        public decimal DiscountPercent { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<OrderCoupon> OrderCoupons { get; set; } = new List<OrderCoupon>();
    }
}
