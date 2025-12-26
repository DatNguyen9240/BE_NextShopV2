using System;
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
                
        [MaxLength(50)]
        public string? GenderTarget { get; set; }
        
        [MaxLength(100)]
        public string? Brand { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public List<string>? Tags { get; set; }

        // Optional: categories to assign to the product on creation
        public List<Guid>? CategoryIds { get; set; }

    }
} 