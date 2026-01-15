using System;
using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class CreateProductVariantRequest
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Base price must be greater than 0")]
        public decimal BasePrice { get; set; }

        [Range(0, 100, ErrorMessage = "Discount percent must be between 0 and 100")]
        public decimal DiscountPercent { get; set; } = 0;

        // Discount amount in the same currency as BasePrice. If provided (>0) it takes precedence over DiscountPercent.
        [Range(0, double.MaxValue, ErrorMessage = "Discount amount must be >= 0")]
        public decimal DiscountAmount { get; set; } = 0;

        [Required]
        public Guid ProductId { get; set; }

        [StringLength(50)]
        public string? SKU { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(20)]
        public string? Size { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; } = 0;

        public bool IsDefault { get; set; } = false;

        // Whether this variant is active/visible
        public bool IsActive { get; set; } = true;

        [Range(0, int.MaxValue)]
        public int DisplayOrder { get; set; } = 0;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(500)]
        public string? ImgHover { get; set; }
    }
}