using System;

namespace NextShopV2.Application.DTOs.Request
{
    public class UpdateAttributeRequest
    {
        public string Name { get; set; } = null!;

        public string? InputType { get; set; }
        public bool IsActive { get; set; }
    }
}