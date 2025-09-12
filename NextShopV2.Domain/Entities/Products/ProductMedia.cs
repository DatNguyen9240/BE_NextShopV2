using System;

namespace NextShopV2.Domain.Entities.Products
{
    public class ProductMedia
    {
        public Guid MediaId { get; set; }
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public string MediaUrl { get; set; } = null!;
        public string MediaType { get; set; } = "Image"; // Image/Video
        public bool IsMain { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Product Product { get; set; } = null!;
        public ProductVariant? Variant { get; set; }
    }
}
