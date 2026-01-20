using System;
using System.Collections.Generic;
using NextShopV2.Domain.Entities.Users;
using NextShopV2.Domain.Entities.Orders;

namespace NextShopV2.Domain.Entities.Orders
{
    public class Shipment
    {
        public Guid ShipmentId { get; set; }
        public Guid OrderId { get; set; }
        public Guid? ShipperId { get; set; }
        public string Carrier { get; set; } = null!;
        public string TrackingNumber { get; set; } = null!;
        public string Status { get; set; } = "Preparing"; // Preparing, In transit, Delivered
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // GPS Tracking
        public double? CurrentLat { get; set; }
        public double? CurrentLng { get; set; }
        public DateTime? LastLocationUpdate { get; set; }

        // Delivery Information
        public string? DeliveryAddress { get; set; }
        public double? DeliveryLat { get; set; }
        public double? DeliveryLng { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;
        public User? Shipper { get; set; }
        public ICollection<TrackingEvent> TrackingEvents { get; set; } = new List<TrackingEvent>();
    }
}