using System;
using NextShopV2.Domain.Entities.Products;

namespace NextShopV2.Domain.Entities.Carts
{
    public class CartItem
    {
        public Guid CartItemId { get; set; }
        public Guid CartId { get; set; }
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }

        public Cart Cart { get; set; } = null!;
        public ProductVariant Variant { get; set; } = null!;
    }
}
