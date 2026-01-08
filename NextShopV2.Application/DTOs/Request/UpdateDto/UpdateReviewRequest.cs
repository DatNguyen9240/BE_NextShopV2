using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class UpdateReviewRequest
    {
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int? Rating { get; set; }

        [StringLength(1000, MinimumLength = 5, ErrorMessage = "Comment must be between 5 and 1000 characters")]
        public string? Comment { get; set; }
    }
}