using System;
using System.Collections.Generic;
using NextShopV2.Domain.Entities.Carts;
using NextShopV2.Domain.Entities.Orders;
using NextShopV2.Domain.Entities.Interactions;
using NextShopV2.Domain.Entities.Products;

namespace NextShopV2.Domain.Entities.Users
{
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? Phone { get; set; }
        public string Role { get; set; } = "User";
        public string? Gender { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<ProductLike> ProductLikes { get; set; } = new List<ProductLike>();
    }
}
