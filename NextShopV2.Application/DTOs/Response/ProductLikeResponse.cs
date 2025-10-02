namespace NextShopV2.Application.DTOs.Response
{
    public class ProductLikeResponse
    {
        public Guid ProductId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ProductName { get; set; } = null!;
    }
}