using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class AssignProductToCategoryRequest
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid CategoryId { get; set; }
    }
}