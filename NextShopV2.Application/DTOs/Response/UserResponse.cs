using System;

namespace NextShopV2.Application.DTOs.Response
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? Avatar { get; set; }
        public List<AddressResponse>? Addresses { get; set; }
    }
}