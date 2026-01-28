using System;
using System.Collections.Generic;

namespace NextShopV2.Application.DTOs.Response
{
    public class OrderResponse
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = null!;
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Buyer info
        public string? BuyerName { get; set; }
        public string? BuyerPhone { get; set; }

        public List<OrderCouponResponse> Coupons { get; set; } = new List<OrderCouponResponse>();
        public string? ShippingAddress { get; set; }
        public double? ShippingLat { get; set; }
        public double? ShippingLng { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new List<OrderItemResponse>();
        public ShipmentResponse? Shipment { get; set; }

        // Cancel info
        public string? CancelReason { get; set; }
        public string? AdminCancelReason { get; set; }
        public string? CancelledBy { get; set; }
    }
    
    public class OrderItemResponse
    {
        public Guid OrderItemId { get; set; }
        public Guid? VariantId { get; set; }
        public Guid? ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TotalAmount { get; set; }
        public ProductVariantResponse? Variant { get; set; }
        // Thêm tên sản phẩm để frontend hiển thị
        public string? ProductName { get; set; }
    }
}