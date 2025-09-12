namespace NextShopV2.Api.Models
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public object? Data { get; set; }
    }
}