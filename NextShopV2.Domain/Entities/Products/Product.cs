using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using NextShopV2.Domain.Entities.Interactions;
namespace NextShopV2.Domain.Entities.Products
{
    public class Product
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? GenderTarget { get; set; }
        public string? Brand { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalLikes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Store tags as JSON in the DB column
        public string? TagsJson { get; set; }

        [NotMapped]
        public List<string> Tags
        {
            get => string.IsNullOrEmpty(TagsJson) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(TagsJson)!;
            set => TagsJson = JsonSerializer.Serialize(value ?? new List<string>());
        }


        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<ProductLike> ProductLikes { get; set; } = new List<ProductLike>();
        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    }
}
