using NextShopV2.Application.DTOs.Request.CreateDto;

namespace NextShopV2.Application.Interfaces.Services
{
    public interface IWebAuthnService
    {
        Task<object> GenerateRegistrationOptionsAsync(Guid userId, string username, string displayName);
        Task<bool> VerifyRegistrationAsync(object attestationResponse, string expectedChallenge, Guid userId);
        Task<object> GenerateAssertionOptionsAsync(Guid userId, IEnumerable<string> allowedCredentials);
        Task<bool> VerifyAuthenticationAsync(object assertionResponse, object options, byte[] publicKey, uint prevCounter);

        // For diagnostics/testing: retrieve stored challenge for a user (base64url string)
        string? GetStoredChallenge(string userId);
    }
}