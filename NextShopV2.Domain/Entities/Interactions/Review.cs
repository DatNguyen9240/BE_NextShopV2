using System;
using NextShopV2.Domain.Entities.Users;
using NextShopV2.Domain.Entities.Products;

namespace NextShopV2.Domain.Entities.Interactions
{
    public class Review
    {
        public Guid ReviewId { get; set; }
        public Guid ProductId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; }      // 1–5
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Product Product { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
