using System;

namespace NextShopV2.Application.DTOs.Request
{
    public class AssignVariantAttributeRequest
    {
        public Guid VariantId { get; set; }
        public Guid AttributeValueId { get; set; }
    }
}