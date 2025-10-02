using System;
using System.Collections.Generic;
using NextShopV2.Domain.Entities.Interactions;
namespace NextShopV2.Domain.Entities.Products
{
    public class Product
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public string? GenderTarget { get; set; }
        public string? Brand { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalLikes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<ProductLike> ProductLikes { get; set; } = new List<ProductLike>();
    }
}
