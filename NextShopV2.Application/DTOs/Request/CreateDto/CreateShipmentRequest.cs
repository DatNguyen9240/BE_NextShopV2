using System;
using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class CreateShipmentRequest
    {
        [Required]
        public Guid OrderId { get; set; }

        public Guid? ShipperId { get; set; }

        [Required]
        [StringLength(100)]
        public string Carrier { get; set; } = "GHN";

        [StringLength(100)]
        public string? TrackingNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Preparing";

        [StringLength(500)]
        public string? DeliveryAddress { get; set; }

        public double? DeliveryLat { get; set; }

        public double? DeliveryLng { get; set; }
    }
}