using System;

namespace NextShopV2.Domain.Entities.Orders
{
    public class OrderCoupon
    {
        public Guid OrderId { get; set; }
        public Guid CouponId { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
