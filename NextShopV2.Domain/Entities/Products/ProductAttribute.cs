using System;
using System.Collections.Generic;

namespace NextShopV2.Domain.Entities.Products
{
    public class ProductAttribute
    {
        public Guid AttributeId { get; set; }
        public string Name { get; set; } = null!;
        public string? InputType { get; set; } // e.g., select, text
        public bool IsActive { get; set; } = true;

        public ICollection<AttributeValue> Values { get; set; } = new List<AttributeValue>();
    }
}