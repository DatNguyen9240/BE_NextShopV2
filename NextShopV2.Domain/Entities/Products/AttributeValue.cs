using System;
using System.Collections.Generic;

namespace NextShopV2.Domain.Entities.Products
{
    public class AttributeValue
    {
        public Guid AttributeValueId { get; set; }
        public Guid AttributeId { get; set; }
        public string Value { get; set; } = null!;
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public ProductAttribute Attribute { get; set; } = null!;
        public ICollection<VariantAttributeValue> VariantAttributeValues { get; set; } = new List<VariantAttributeValue>();
    }
}