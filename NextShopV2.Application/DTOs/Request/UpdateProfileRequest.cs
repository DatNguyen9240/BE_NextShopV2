using System;

namespace NextShopV2.Application.DTOs.Request
{
    public class UpdateProfileRequest
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public string? AvatarUrl { get; set; }
    }
}