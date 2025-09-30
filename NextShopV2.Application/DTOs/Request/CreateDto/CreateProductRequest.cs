using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class CreateProductRequest
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal BasePrice { get; set; }
        
        [MaxLength(50)]
        public string? GenderTarget { get; set; }
        
        [MaxLength(100)]
        public string? Brand { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // KHÔNG CÓ productId - server sẽ tự generate
        // KHÔNG CÓ averageRating, totalReviews, totalLikes - sẽ được tính toán
    }
}