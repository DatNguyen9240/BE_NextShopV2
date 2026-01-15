using System;
using NextShopV2.Domain.Entities.Products;

namespace NextShopV2.Domain.Entities.Orders
{
    public class OrderItem
    {
        public Guid OrderItemId { get; set; }
        public Guid OrderId { get; set; }

        // Nullable Variant/Product references to allow hard-delete of products/variants
        public Guid? VariantId { get; set; }
        public Guid? ProductId { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Snapshot fields to preserve order history
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        // Tax rate applied to this line (e.g., 0.07 = 7%)
        public decimal TaxRate { get; set; }
        public decimal TotalAmount { get; set; }

        public string? ProductName { get; set; }
        public string? ProductSku { get; set; }
        public string? VariantSku { get; set; }
        public string? VariantOptionsJson { get; set; }

        public Order Order { get; set; } = null!;
        public ProductVariant? Variant { get; set; }
    }
} 
