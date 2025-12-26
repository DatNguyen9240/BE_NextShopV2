using System;

namespace NextShopV2.Application.DTOs.Response
{
    public class ProductVariantResponse
    {
        public decimal BasePrice { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal PriceAfterDiscount { get; set; }
        public Guid ProductVariantId { get; set; }
        public Guid ProductId { get; set; }
        public string Sku { get; set; } = null!;
        public string? Color { get; set; }
        public string? Size { get; set; }
        public int StockQuantity { get; set; }
        public bool IsDefault { get; set; }
        public int DisplayOrder { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImgHover { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}