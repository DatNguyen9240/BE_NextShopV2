using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class BulkAssignProductToCategoriesRequest
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public List<Guid> CategoryIds { get; set; } = new List<Guid>();
    }
}