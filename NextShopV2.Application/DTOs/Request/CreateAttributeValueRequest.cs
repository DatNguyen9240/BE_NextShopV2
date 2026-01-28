using System;

namespace NextShopV2.Application.DTOs.Request
{
    public class CreateAttributeValueRequest
    {
        public Guid AttributeId { get; set; }
        public string Value { get; set; } = null!;
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}