using System;

namespace NextShopV2.Domain.Entities.Payments
{
    public class Shipment
    {
        public Guid ShipmentId { get; set; }
        public Guid OrderId { get; set; }
        public string Carrier { get; set; } = "GHN";
        public string TrackingNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "Preparing";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
