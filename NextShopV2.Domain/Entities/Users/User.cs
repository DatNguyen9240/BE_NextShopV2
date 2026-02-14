using System;
using System.Collections.Generic;
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
        // Optional avatar URL
        public string? Avatar { get; set; }

        // Google sign-in and email verification
        public string? GoogleId { get; set; }
        public bool EmailVerified { get; set; } = false;

        // Multi-factor auth settings
        public bool MfaEnabled { get; set; } = false;
        public string? MfaType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Soft-delete fields (commented out due to migration issues)
        // public bool IsDeleted { get; set; } = false;
        // public DateTime? DeletedAt { get; set; }

        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<ProductLike> ProductLikes { get; set; } = new List<ProductLike>();
    }
}
