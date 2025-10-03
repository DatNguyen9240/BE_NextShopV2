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
        public decimal? MinOrderAmount { get; set; } // Số tiền tối thiểu để áp dụng coupon
        public decimal? MaxDiscountAmount { get; set; } // Số tiền giảm tối đa
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public int? UsageLimit { get; set; } // Giới hạn số lần sử dụng
        public int UsedCount { get; set; } = 0; // Số lần đã sử dụng
        public bool IsActive { get; set; } = true;
    }
}
