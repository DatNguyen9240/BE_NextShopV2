using System;

namespace NextShopV2.Application.DTOs.Response
{
    public class ProductVariantResponse
    {
        public Guid ProductVariantId { get; set; }
        public Guid ProductId { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public decimal AdditionalPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsDefault { get; set; }
        public int DisplayOrder { get; set; }
        public string? ImageUrl { get; set; }
    }
}