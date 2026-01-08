using System;
using System.Collections.Generic;

namespace NextShopV2.Domain.Entities.Products
{
    public class ProductVariant
    {
        public Guid VariantId { get; set; }
        public Guid ProductId { get; set; }
        public string SKU { get; set; } = null!;
        public string? Color { get; set; }
        public string? Size { get; set; }
        public decimal BasePrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal PriceAfterDiscount { get; set; }
        public int StockQuantity { get; set; }
        public bool IsDefault { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        public string? ImageUrl { get; set; }
        public string? ImgHover { get; set; } // Image hiển thị khi hover
        public string? ThumbnailUrl { get; set; }
        public bool IsActive { get; set; } = false;

        public Product Product { get; set; } = null!;
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
    }
}
