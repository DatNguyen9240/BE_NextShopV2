using System;
using System.Collections.Generic;
using NextShopV2.Domain.Entities.Users;
using NextShopV2.Domain.Entities.Payments;
using NextShopV2.Domain.Entities.Coupons;

namespace NextShopV2.Domain.Entities.Orders
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Pending, Paid, Shipped, Completed, Cancelled
        public string? CancelReason { get; set; }
        public string? AdminCancelReason { get; set; }
        public string? CancelledBy { get; set; }
        public DateTime? CancelledAt { get; set; }
        public decimal SubTotal { get; set; } // Total before discount and tax (price only)
        public decimal TaxAmount { get; set; } = 0; // Total tax amount (calculated on price after discount)
        public decimal DiscountAmount { get; set; } = 0; // Total discount applied (on price only)
        public decimal TotalAmount { get; set; } // Final amount: (SubTotal - DiscountAmount) + TaxAmount

        // Buyer information (for payment records)
        public string? BuyerName { get; set; }
        public string? BuyerPhone { get; set; }
        public string? ShippingAddress { get; set; }
        public double? ShippingLat { get; set; }
        public double? ShippingLng { get; set; }

        public User User { get; set; } = null!;
        public ICollection<OrderCoupon> OrderCoupons { get; set; } = new List<OrderCoupon>();
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public Shipment? Shipment { get; set; }
    }
}
