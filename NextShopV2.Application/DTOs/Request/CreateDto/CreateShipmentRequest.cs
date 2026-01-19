using System;
using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class CreateShipmentRequest
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        [StringLength(100)]
        public string Carrier { get; set; } = "GHN";

        [StringLength(100)]
        public string? TrackingNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Preparing";
    }
}