using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class CreateCategoryRequest
    {
        public string? ImageUrl { get; set; }
        public string? Icon { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public Guid? ParentId { get; set; }
        
        public decimal? TaxRate { get; set; }
    }
}