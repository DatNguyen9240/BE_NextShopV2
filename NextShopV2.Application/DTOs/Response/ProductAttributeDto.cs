using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NextShopV2.Application.DTOs.Response
{
    public class ProductAttributeDto
    {
        public Guid AttributeId { get; set; }
        public string Name { get; set; } = null!;
        public string? InputType { get; set; }
        public bool IsActive { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<AttributeValueDto>? Values { get; set; }
    }
}