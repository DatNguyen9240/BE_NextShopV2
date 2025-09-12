using System;

namespace NextShopV2.Domain.Entities.Users
{
    public class Address
    {
        public Guid AddressId { get; set; }
        public Guid UserId { get; set; }
        public string RecipientName { get; set; } = null!;
        public string Street { get; set; } = null!;
        public string City { get; set; } = null!;
        public string District { get; set; } = null!;
        public string Ward { get; set; } = null!;
        public string? PostalCode { get; set; }
        public bool IsDefault { get; set; }

        public User User { get; set; } = null!;
    }
}
