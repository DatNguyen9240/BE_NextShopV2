using System;

namespace NextShopV2.Application.DTOs.Response
{
    // Simplified variant info for cart
    public class VariantInfo
    {
        public Guid ProductId { get; set; }
        public decimal Price { get; set; }
        public string ProductName { get; set; } = string.Empty;
        // attribute name -> value map
        public System.Collections.Generic.Dictionary<string, string> Attributes { get; set; } = new();
        public string ImageUrl { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public bool ProductIsActive { get; set; }
    }
} 