using System;
using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class CreateProductVariantRequest
    {
        [Required]
        public Guid ProductId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = null!;
        
        [StringLength(50)]
        public string? Color { get; set; }
        
        [StringLength(20)]
        public string? Size { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal AdditionalPrice { get; set; } = 0;
        
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; } = 0;
        
        public bool IsDefault { get; set; } = false;
        
        [Range(0, int.MaxValue)]
        public int DisplayOrder { get; set; } = 0;
        
        [StringLength(500)]
        public string? ImageUrl { get; set; }
    }
}