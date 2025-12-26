using System;
using System.Collections.Generic;

namespace NextShopV2.Application.DTOs.Response
{
    public class ProductDto
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? GenderTarget { get; set; }
        public string? Brand { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalLikes { get; set; }
        public bool IsActive { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<ProductVariantResponse> Variants { get; set; } = new List<ProductVariantResponse>();
        public List<string> Images { get; set; } = new List<string>();
        public List<Guid> CategoryIds { get; set; } = new List<Guid>();
    }
}
