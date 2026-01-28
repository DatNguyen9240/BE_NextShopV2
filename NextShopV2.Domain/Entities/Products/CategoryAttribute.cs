using System;

namespace NextShopV2.Domain.Entities.Products
{
    public class CategoryAttribute
    {
        public Guid CategoryId { get; set; }
        public Guid AttributeId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public Category Category { get; set; } = null!;
        public ProductAttribute Attribute { get; set; } = null!;
    }
}