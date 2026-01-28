using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace NextShopV2.Application.DTOs.Response
{
    public class ProductVariantResponse
    {
        public decimal BasePrice { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal PriceAfterDiscount { get; set; }
        public Guid ProductVariantId { get; set; }
        public string Sku { get; set; } = null!;
        public int StockQuantity { get; set; }
        public bool IsDefault { get; set; }
        public int DisplayOrder { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImgHover { get; set; }
        // attribute name -> value map for dynamic variant attributes
        public Dictionary<string, string>? Attributes { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsActive { get; set; }
    }
}