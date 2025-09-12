using System;

namespace NextShopV2.Domain.Entities.Products
{
    public class ProductCategory
    {
        public Guid ProductId { get; set; }
        public Guid CategoryId { get; set; }

        public Product Product { get; set; } = null!;
        public Category Category { get; set; } = null!;
    }
}
