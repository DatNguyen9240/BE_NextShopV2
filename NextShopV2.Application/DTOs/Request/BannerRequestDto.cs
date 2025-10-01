namespace NextShopV2.Application.DTOs.Request.CreateDto
{
    public class BannerRequestDto
    {
        public string? Title { get; set; }
        public string? PublicId { get; set; }
        public string? ImageUrl { get; set; }
        public string? Type { get; set; }
        public int? SortOrder { get; set; }
        // Không có Id, CreatedAt
    }
}
