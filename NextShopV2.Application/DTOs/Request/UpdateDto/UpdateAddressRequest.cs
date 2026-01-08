using System;

namespace NextShopV2.Application.DTOs.Request
{
    public class UpdateAddressRequest
    {
        // Accept addressId as a string so we can be tolerant of non-GUID client values
        // (e.g., place identifiers). If it's not a valid GUID we'll treat it as "create new".
        public string? AddressId { get; set; }
        public string FullAddress { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsDefault { get; set; } = true;
    }
}