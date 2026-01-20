using System;

namespace NextShopV2.Application.DTOs.Response
{
    public class ShipmentLocationResponse
    {
        public Guid ShipmentId { get; set; }
        public double? CurrentLat { get; set; }
        public double? CurrentLng { get; set; }
        public DateTime? LastLocationUpdate { get; set; }
        public string Status { get; set; } = null!;
    }
}