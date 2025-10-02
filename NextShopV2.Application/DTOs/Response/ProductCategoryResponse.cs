namespace NextShopV2.Application.DTOs.Response
{
    public class ProductCategoryResponse
    {
        public Guid ProductId { get; set; }
        public Guid CategoryId { get; set; }
        public string ProductName { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public DateTime AssignedAt { get; set; }
    }
}