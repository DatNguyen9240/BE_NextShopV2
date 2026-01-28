using System;

namespace NextShopV2.Application.DTOs.Response
{
    public class AttributeValueDto
    {
        public Guid AttributeValueId { get; set; }
        public Guid AttributeId { get; set; }
        public string Value { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}