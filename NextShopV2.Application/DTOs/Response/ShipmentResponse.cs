using System;
using System.Collections.Generic;

namespace NextShopV2.Application.DTOs.Response
{
    public class ShipmentResponse
    {
        public Guid ShipmentId { get; set; }
        public Guid OrderId { get; set; }
        public Guid? ShipperId { get; set; }
        public UserResponse? Shipper { get; set; }
        public string Carrier { get; set; } = null!;
        public string TrackingNumber { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        // GPS Tracking
        public double? CurrentLat { get; set; }
        public double? CurrentLng { get; set; }
        public DateTime? LastLocationUpdate { get; set; }

        // Delivery Information
        public string? DeliveryAddress { get; set; }
        public double? DeliveryLat { get; set; }
        public double? DeliveryLng { get; set; }

        public List<TrackingEventResponse> TrackingEvents { get; set; } = new List<TrackingEventResponse>();
    }
}