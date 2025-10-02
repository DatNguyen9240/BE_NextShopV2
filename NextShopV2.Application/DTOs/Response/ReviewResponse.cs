namespace NextShopV2.Application.DTOs.Response
{
    public class ReviewResponse
    {
        public Guid ReviewId { get; set; }
        public Guid ProductId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string ProductName { get; set; } = null!;
        public string UserName { get; set; } = null!;
    }
}