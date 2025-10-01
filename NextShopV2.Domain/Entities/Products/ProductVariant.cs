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
        public decimal AdditionalPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsDefault { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        public string? ImageUrl { get; set; }

        public Product Product { get; set; } = null!;
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
    }
}
