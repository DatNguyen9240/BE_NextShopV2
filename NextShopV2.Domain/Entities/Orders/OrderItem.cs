using System;
using NextShopV2.Domain.Entities.Products;

namespace NextShopV2.Domain.Entities.Orders
{
    public class OrderItem
    {
        public Guid OrderItemId { get; set; }
        public Guid OrderId { get; set; }
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public Order Order { get; set; } = null!;
        public ProductVariant Variant { get; set; } = null!;
    }
}
