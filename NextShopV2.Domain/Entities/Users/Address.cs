using System;

namespace NextShopV2.Domain.Entities.Users
{
    public class Address
    {
        public Guid AddressId { get; set; }
        public Guid UserId { get; set; }
        public string RecipientName { get; set; } = null!;
        // Single-line address input (e.g., "123 Main St, Ward, District, City")
        public string FullAddress { get; set; } = null!;
        // Optional latitude & longitude for geolocation
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsDefault { get; set; }

        public User User { get; set; } = null!;
    }
}
