using System;
using System.Collections.Generic;
using NextShopV2.Domain.Entities.Users;
using NextShopV2.Domain.Entities.Payments;

namespace NextShopV2.Domain.Entities.Orders
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Pending, Paid, Shipped, Completed, Cancelled
        public decimal SubTotal { get; set; } // Total before discount
        public decimal DiscountAmount { get; set; } = 0; // Total discount applied
        public decimal TotalAmount { get; set; } // Final amount after discount
        public string? CouponCode { get; set; } // Applied coupon code
        public string? ShippingAddress { get; set; }

        public User User { get; set; } = null!;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<OrderCoupon> Coupons { get; set; } = new List<OrderCoupon>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public Shipment? Shipment { get; set; }
    }
}
