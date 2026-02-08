using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NextShopV2.Application.DTOs.Response
{
    public class ProductDto
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AdditionalInfo { get; set; }
        public string? GenderTarget { get; set; }
        public string? Brand { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalLikes { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsActive { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? TaxRate { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Tags { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ProductVariantResponse>? Variants { get; set; }
        /// <summary>
        /// Total stock available for this product (sum of all variants' StockQuantity)
        /// </summary>
        public int TotalStockQuantity { get; set; }

        // Removed `Images`, `CategoryNames`, and `CategoryIds` per API contract — variant-level `ImgHover` will be returned instead
    }
}
