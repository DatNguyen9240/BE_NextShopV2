using System;
using NextShopV2.Domain.Entities.Users;
using NextShopV2.Domain.Entities.Products;

namespace NextShopV2.Domain.Entities.Interactions
{
    public class ProductLike
    {
        public Guid ProductId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Product Product { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
