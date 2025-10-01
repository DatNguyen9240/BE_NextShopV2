using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class UpdateOrderStatusRequest
    {
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = null!; // Pending, Paid, Shipped, Completed, Cancelled
    }
}