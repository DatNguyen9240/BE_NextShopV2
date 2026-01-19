using System;

namespace NextShopV2.Domain.Entities.Payments
{
    public class TrackingEvent
    {
        public Guid TrackingEventId { get; set; }
        public Guid ShipmentId { get; set; }
        public string Status { get; set; } = null!; // "Picked up", "In transit", "Out for delivery", "Delivered"
        public string Description { get; set; } = null!;
        public string? Location { get; set; }
        public DateTime EventTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Shipment Shipment { get; set; } = null!;
    }
}