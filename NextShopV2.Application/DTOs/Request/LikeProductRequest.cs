using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class LikeProductRequest
    {
        [Required]
        public Guid ProductId { get; set; }
    }
}