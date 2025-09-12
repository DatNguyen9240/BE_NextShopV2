using System;
using System.Collections.Generic;
using NextShopV2.Domain.Entities.Users;
using NextShopV2.Domain.Entities.Products;

namespace NextShopV2.Domain.Entities.Carts
{
    public class Cart
    {
        public Guid CartId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
