using System;

namespace NextShopV2.Application.DTOs.Request
{
    public class UpdateAttributeValueRequest
    {
        public string Value { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}