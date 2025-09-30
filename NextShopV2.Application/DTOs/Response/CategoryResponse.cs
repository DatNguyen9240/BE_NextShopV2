namespace NextShopV2.Application.DTOs.Response
{
    public class CategoryResponse
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public Guid? ParentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ParentName { get; set; }
        public List<CategoryResponse> Children { get; set; } = new List<CategoryResponse>();
    }
}