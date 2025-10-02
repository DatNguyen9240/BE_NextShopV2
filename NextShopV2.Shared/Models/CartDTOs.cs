using System;
using System.Collections.Generic;
using System.Linq;

namespace NextShopV2.Shared.Models
{
    public class CartDto
    {
        public Guid CartId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(x => x.TotalPrice);
        public int TotalItems => Items.Sum(x => x.Quantity);
    }

    public class CartItemDto
    {
        public Guid CartItemId { get; set; }
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        public string ProductName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
    }

    public class AddCartItemDto
    {
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartItemDto
    {
        public int Quantity { get; set; }
    }

    // Internal Redis storage model
    public class RedisCartItem
    {
        public Guid CartItemId { get; set; }
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }
    }
}