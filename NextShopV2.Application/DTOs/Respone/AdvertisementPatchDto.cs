namespace NextShopV2.Application.DTOs.Response
{
    public class AdvertisementPatchDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? MediaUrl { get; set; }
        public string? TargetUrl { get; set; }
        public int? SortOrder { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsActive { get; set; }
    }
}