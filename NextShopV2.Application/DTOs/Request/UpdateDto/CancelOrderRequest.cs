using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class CancelOrderRequest
    {
        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(500)]
        public string? AdminReason { get; set; }
    }
}