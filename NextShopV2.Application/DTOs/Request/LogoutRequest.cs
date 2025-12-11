namespace NextShopV2.Application.DTOs.Request
{
    public class LogoutRequest
    {
        // Optional access token in body (fallback if Authorization header is not provided)
        public string AccessToken { get; set; }

        // Optional refresh token to also revoke
        public string RefreshToken { get; set; }
    }
}
