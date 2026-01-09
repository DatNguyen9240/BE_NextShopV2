namespace NextShopV2.Api.Models
{
    public class NotificationDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string? Body { get; set; }
        public string? Url { get; set; }
        public bool Read { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
