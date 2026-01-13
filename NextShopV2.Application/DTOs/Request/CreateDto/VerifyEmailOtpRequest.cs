namespace NextShopV2.Application.DTOs.Request
{
    public class VerifyEmailOtpRequest
    {
        public string? RequestId { get; set; }
        public string? Code { get; set; }
    }
}