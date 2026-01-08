using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class CreateOrderRequest
    {
        [Required]
        [MinLength(1)]
        public List<CreateOrderItemRequest> Items { get; set; } = new List<CreateOrderItemRequest>();
        
        [StringLength(200)]
        public string? BuyerName { get; set; }

        [StringLength(50)]
        public string? BuyerPhone { get; set; }

        [StringLength(500)]
        public string? ShippingAddress { get; set; }
        
    // Hỗ trợ nhiều coupon cho mỗi order
    public List<Guid>? CouponIds { get; set; }
    }
    
    public class CreateOrderItemRequest
    {
        [Required]
        public Guid VariantId { get; set; }
        
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}