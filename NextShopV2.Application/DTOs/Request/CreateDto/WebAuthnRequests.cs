namespace NextShopV2.Application.DTOs.Request.CreateDto
{
    public class RegisterOptionsRequest
    {
        public Guid UserId { get; set; }
    }

    public class VerifyRegistrationRequest
    {
        public Guid UserId { get; set; }
        public object Credential { get; set; } = new { };
        public string Challenge { get; set; } = string.Empty;
    }

    public class LoginOptionsRequest
    {
        public Guid? UserId { get; set; }
        public string? Email { get; set; }
    }

    public class VerifyLoginRequest
    {
        public object Assertion { get; set; } = new { };
        public string Challenge { get; set; } = string.Empty;
    }
}