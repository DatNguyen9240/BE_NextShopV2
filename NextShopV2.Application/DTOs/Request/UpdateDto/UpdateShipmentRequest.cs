using System;

namespace NextShopV2.Application.DTOs.Request.UpdateDto
{
    public class UpdateShipmentRequest
    {
        public string? Carrier { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Status { get; set; }
        public Guid? ShipperId { get; set; }
    }
}