using System;
using System.Collections.Generic;

namespace NextShopV2.Domain.Entities.Payments
{
    public class Shipment
    {
        public Guid ShipmentId { get; set; }
        public Guid OrderId { get; set; }
        public Guid? ShipperId { get; set; } // Assigned shipper
        public string Carrier { get; set; } = "GHN";
        public string TrackingNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "Preparing";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // GPS Tracking
        public double? CurrentLat { get; set; }
        public double? CurrentLng { get; set; }
        public DateTime? LastLocationUpdate { get; set; }

        // Navigation
        public ICollection<TrackingEvent> TrackingEvents { get; set; } = new List<TrackingEvent>();
    }
}
