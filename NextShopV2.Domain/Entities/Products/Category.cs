using System;
using System.Collections.Generic;

namespace NextShopV2.Domain.Entities.Products
{
    public class Category
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public Guid? ParentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ImageUrl { get; set; }
        public string? Icon { get; set; }

        // Tax rate for products in this category (nullable - if null, use system default)
        public decimal? TaxRate { get; set; }

        public Category? Parent { get; set; }
        public ICollection<Category> Children { get; set; } = new List<Category>();

    }
}
