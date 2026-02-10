using System;

namespace NextShopV2.Application.DTOs.Request
{
    public class CreateAttributeRequest
    {
        public string Name { get; set; } = null!;
        public string? InputType { get; set; }
        public bool IsActive { get; set; } = true;
    }
}