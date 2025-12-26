using System;
using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class UpdateProductRequest
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;
        
        [MaxLength(1000)]
        public string? Description { get; set; }
                
        [MaxLength(50)]
        public string? GenderTarget { get; set; }
        
        [MaxLength(100)]
        public string? Brand { get; set; }
        
        public bool IsActive { get; set; }

        public List<string>? Tags { get; set; }

        // Optional: replace product categories with these (bulk assign)
        public List<Guid>? CategoryIds { get; set; }
        
    }
}  