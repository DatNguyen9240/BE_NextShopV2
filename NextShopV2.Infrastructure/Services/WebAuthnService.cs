using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NextShopV2.Application.Interfaces.Services;
using System.Text;

namespace NextShopV2.Infrastructure.Services
{
    // NOTE: This is a minimal placeholder implementation to allow builds and DB migrations.
    public class WebAuthnService : IWebAuthnService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<WebAuthnService> _logger;
        private readonly string _origin;
        private readonly string _rpId;

        public WebAuthnService(IMemoryCache cache, IConfiguration config, ILogger<WebAuthnService> logger)
        {
            _cache = cache;
            _logger = logger;
            _origin = config["Fido2:Origin"] ?? "https://localhost:3000";
            _rpId = config["Fido2:RPID"] ?? config["App:Domain"] ?? "localhost";
        }

        public Task<object> GenerateRegistrationOptionsAsync(Guid userId, string username, string displayName)
        {
            // Use base64url-encoded values so client utilities can decode to ArrayBuffers
            var challengeBytes = System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
            var challenge = Base64UrlEncode(challengeBytes);
            var userIdBase64 = Base64UrlEncode(userId.ToByteArray());
            // Cache challenge per user
            _cache.Set(GetChallengeCacheKey(userId.ToString()), challenge, TimeSpan.FromMinutes(5));

            var options = new
            {
                challenge,
                rp = new { name = _rpId },
                user = new { id = userIdBase64, name = username, displayName = displayName },
                pubKeyCredParams = new[] { new { type = "public-key", alg = -7 }, new { type = "public-key", alg = -257 } },
                timeout = 60000,
                attestation = "none"
            };

            return Task.FromResult<object>(options);
        }

        public Task<bool> VerifyRegistrationAsync(object attestationResponse, string expectedChallenge, Guid userId)
        {
            var storedChallenge = _cache.Get<string>(GetChallengeCacheKey(userId.ToString()));
            if (storedChallenge == null || storedChallenge != expectedChallenge) throw new InvalidOperationException("Challenge mismatch or expired");
            // NOTE: This is a placeholder. Replace with proper FIDO2 attestation verification when integrating Fido2NetLib.
            // For now, assume attestation is valid to enable manual/testing flows.
            return Task.FromResult(true);
        }

        public async Task<object> GenerateAssertionOptionsAsync(Guid userId, IEnumerable<string> allowedCredentials)
        {
            var challengeBytes = System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
            var challenge = Base64UrlEncode(challengeBytes);
            // Cache challenge per user
            _cache.Set(GetChallengeCacheKey(userId.ToString()), challenge, TimeSpan.FromMinutes(5));
            var allowCredentials = allowedCredentials == null ? new List<object>() : allowedCredentials.Select(c => (object)new { type = "public-key", id = c }).ToList();
            return await Task.FromResult<object>(new { challenge, allowCredentials, timeout = 60000, rpId = _rpId });
        }

        public async Task<bool> VerifyAuthenticationAsync(object assertionResponse, object options, byte[] publicKey, uint prevCounter)
        {
            // options expected to contain { challenge: string, userId: Guid }
            try
            {
                string? incomingChallenge = null;
                string? userIdStr = null;

                if (options is System.Text.Json.JsonElement je)
                {
                    if (je.TryGetProperty("challenge", out var ch) && ch.ValueKind == System.Text.Json.JsonValueKind.String) incomingChallenge = ch.GetString();
                    if (je.TryGetProperty("userId", out var uid) && uid.ValueKind == System.Text.Json.JsonValueKind.String) userIdStr = uid.GetString();
                }
                else if (options is string s)
                {
                    incomingChallenge = s;
                }

                if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var userId))
                {
                    var stored = _cache.Get<string>(GetChallengeCacheKey(userId.ToString()));

                    // Normalize base64url strings before comparison
                    string Normalize(string? x)
                    {
                        if (string.IsNullOrEmpty(x)) return string.Empty;
                        var t = x.Replace('-', '+').Replace('_', '/');
                        switch (t.Length % 4)
                        {
                            case 2: t += "=="; break;
                            case 3: t += "="; break;
                        }
                        return t;
                    }

                    var normStored = Normalize(stored);
                    var normIncoming = Normalize(incomingChallenge);

                    bool ok = false;
                    if (!string.IsNullOrEmpty(normStored) && !string.IsNullOrEmpty(normIncoming))
                    {
                        ok = normStored == normIncoming;
                    }

                    _logger.LogInformation("VerifyAuthentication: user {UserId} normStored={NormStored} normIncoming={NormIncoming} => ok={Ok}", userId, normStored, normIncoming, ok);
                    return await Task.FromResult(ok);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VerifyAuthentication error");
            }

            // Fallback failure
            return await Task.FromResult(false);
        }

        private string GetChallengeCacheKey(string userId) => $"webauthn:challenge:{userId}";

        public string? GetStoredChallenge(string userId) => _cache.Get<string>(GetChallengeCacheKey(userId));

        private static string Base64UrlEncode(byte[] input)
        {
            var s = Convert.ToBase64String(input);
            s = s.Split('=')[0]; // Remove any trailing '='s
            s = s.Replace('+', '-'); // 62nd char of encoding
            s = s.Replace('/', '_'); // 63rd char of encoding
            return s;
        }

    }
}