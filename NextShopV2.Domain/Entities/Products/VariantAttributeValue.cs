using System;

namespace NextShopV2.Domain.Entities.Products
{
    public class VariantAttributeValue
    {
        public Guid VariantId { get; set; }
        public Guid AttributeValueId { get; set; }

        public ProductVariant Variant { get; set; } = null!;
        public AttributeValue AttributeValue { get; set; } = null!;
    }
}