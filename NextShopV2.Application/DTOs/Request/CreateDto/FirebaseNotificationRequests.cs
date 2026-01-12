namespace NextShopV2.Application.DTOs.Request.CreateDto
{
    public class FirebaseNotificationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public Dictionary<string, string>? Data { get; set; }
    }

    public class SendNotificationRequest : FirebaseNotificationRequest
    {
        public string? Token { get; set; }
    }

    public class TopicNotificationRequest : FirebaseNotificationRequest
    {
        public string? Topic { get; set; }
    }

    public class FirebaseFcmTokenModel
    {
        public string Token { get; set; } = string.Empty;
        public string? UserId { get; set; }
    }

    public class FirebaseFcmTokenDto
    {
        public string Token { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FirebaseNotificationHistoryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Body { get; set; }
        public string? ImageUrl { get; set; }
        public string? Data { get; set; }
        public int RecipientCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public DateTime SentAt { get; set; }
    }

    public class FirebaseNotificationSendResult
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> InvalidTokens { get; set; } = new List<string>();
        public string Summary { get; set; } = string.Empty;
    }
}