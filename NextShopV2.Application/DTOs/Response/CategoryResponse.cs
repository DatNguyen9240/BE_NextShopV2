namespace NextShopV2.Application.DTOs.Response
{
    public class CategoryResponse
    {
        public string? ImageUrl { get; set; }
        public string? Icon { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public Guid? ParentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ParentName { get; set; }
        public List<CategoryResponse> Children { get; set; } = new List<CategoryResponse>();
    }
}