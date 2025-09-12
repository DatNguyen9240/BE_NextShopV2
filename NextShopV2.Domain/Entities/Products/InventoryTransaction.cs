using System;

namespace NextShopV2.Domain.Entities.Products
{
    public class InventoryTransaction
    {
        public Guid TransactionId { get; set; }
        public Guid VariantId { get; set; }
        public int ChangeQty { get; set; }
        public string Reason { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = null!;

        public ProductVariant Variant { get; set; } = null!;
    }
}
