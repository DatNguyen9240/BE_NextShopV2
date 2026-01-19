using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class UpdateShipmentLocationRequest
    {
        [Required]
        public double Lat { get; set; }

        [Required]
        public double Lng { get; set; }
    }
}