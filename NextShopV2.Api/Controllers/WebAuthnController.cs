using System.Text;
using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.DTOs.Request.CreateDto;
using NextShopV2.Application.Interfaces;
using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Domain.Entities.Security;
using NextShopV2.Shared.Helpers;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/webauthn")]
    public class WebAuthnController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasskeyRepository _passkeyRepository;
        private readonly IWebAuthnService _webauthnService;
        private readonly IConfiguration _config;
        private readonly ILogger<WebAuthnController> _logger;
        private readonly IDatabase _redisDb;

        public WebAuthnController(IUserRepository userRepository, IPasskeyRepository passkeyRepository, IWebAuthnService webauthnService, IConfiguration config, ILogger<WebAuthnController> logger, IConnectionMultiplexer redis)
        {
            _userRepository = userRepository;
            _passkeyRepository = passkeyRepository;
            _webauthnService = webauthnService;
            _config = config;
            _logger = logger;
            _redisDb = redis.GetDatabase();
        }

        [HttpPost("register/options")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> RegisterOptions([FromBody] RegisterOptionsRequest? req)
        {
            Guid userId;
            if (req != null && req.UserId != Guid.Empty)
            {
                userId = req.UserId;
            }
            else
            {
                var claim = User.FindFirst("userId")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out userId)) return BadRequest("User not found");
            }

            var user = _userRepository.GetById(userId);
            if (user == null) return BadRequest("User not found");

            var options = await _webauthnService.GenerateRegistrationOptionsAsync(userId, user.Email, user.FullName ?? user.Email);

            return Ok(options);
        }

        [HttpPost("register/verify")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> RegisterVerify([FromBody] System.Text.Json.JsonElement body)
        {
            // Parse userId from body or fallback to claims
            Guid userId;
            if (body.TryGetProperty("userId", out var userIdElem) && userIdElem.ValueKind == System.Text.Json.JsonValueKind.String && Guid.TryParse(userIdElem.GetString(), out var parsed) && parsed != Guid.Empty)
            {
                userId = parsed;
            }
            else
            {
                var claim = User.FindFirst("userId")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out userId)) return BadRequest("User not found");
            }

            var user = _userRepository.GetById(userId);
            if (user == null) {
                _logger.LogWarning("RegisterVerify: user not found for id {UserId}", userId);
                return BadRequest("User not found");
            }

            // Extract challenge and credential from body
            var challenge = body.TryGetProperty("challenge", out var chElem) && chElem.ValueKind == System.Text.Json.JsonValueKind.String ? chElem.GetString() ?? string.Empty : string.Empty;
            var credential = body.TryGetProperty("credential", out var credElem) ? credElem : default;

            _logger.LogInformation("RegisterVerify called for user {UserId} with challenge {Challenge}. Body: {Body}", userId, challenge, body.GetRawText());

            if (string.IsNullOrEmpty(challenge))
            {
                _logger.LogWarning("RegisterVerify: missing challenge in request for user {UserId}", userId);
                return BadRequest(new { success = false, reason = "missing challenge" });
            }

            try
            {
                var verified = await _webauthnService.VerifyRegistrationAsync(credential, challenge, userId);
                if (verified)
                {
                    // Extract registration info and persist passkey (minimal implementation)
                    try
                    {
                        if (credential.ValueKind == System.Text.Json.JsonValueKind.Object)
                        {
                            string? rawId = null;
                            if (credential.TryGetProperty("rawId", out var rawIdElem) && rawIdElem.ValueKind == System.Text.Json.JsonValueKind.String) rawId = rawIdElem.GetString();
                            else if (credential.TryGetProperty("id", out var idElem) && idElem.ValueKind == System.Text.Json.JsonValueKind.String) rawId = idElem.GetString();

                            string? attObj = null;
                            if (credential.TryGetProperty("response", out var resp) && resp.ValueKind == System.Text.Json.JsonValueKind.Object && resp.TryGetProperty("attestationObject", out var attElem) && attElem.ValueKind == System.Text.Json.JsonValueKind.String)
                                attObj = attElem.GetString();

                            string? transports = null;
                            if (credential.TryGetProperty("transports", out var trav) && trav.ValueKind != System.Text.Json.JsonValueKind.Undefined) transports = trav.GetRawText();

                            if (!string.IsNullOrEmpty(rawId))
                            {
                                // If attestationObject present, try to convert it to base64 and store as PublicKey placeholder
                                string publicKeyBase64 = string.Empty;
                                if (!string.IsNullOrEmpty(attObj))
                                {
                                    var t = attObj.Replace('-', '+').Replace('_', '/');
                                    switch (t.Length % 4)
                                    {
                                        case 2: t += "=="; break;
                                        case 3: t += "="; break;
                                    }
                                    try { var bytes = Convert.FromBase64String(t); publicKeyBase64 = Convert.ToBase64String(bytes); }
                                    catch { publicKeyBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(attObj)); }
                                }

                                var passkey = new NextShopV2.Domain.Entities.Security.Passkey
                                {
                                    UserId = userId,
                                    CredentialId = rawId,
                                    PublicKey = publicKeyBase64,
                                    Counter = 0,
                                    Transports = transports
                                };

                                var existing = await _passkeyRepository.GetByCredentialIdAsync(rawId);
                                if (existing == null)
                                {
                                    await _passkeyRepository.AddAsync(passkey);
                                    _logger.LogInformation("Registered new passkey for user {UserId}, credential {Cred}", userId, rawId);
                                }
                                else
                                {
                                    _logger.LogInformation("Passkey already exists for credential {Cred}", rawId);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to persist passkey for user {UserId}", userId);
                        // continue; even if saving fails, return success for now
                    }

                    return Ok(new { success = true });
                }

                _logger.LogWarning("RegisterVerify: verification returned false for user {UserId}", userId);
                // Include stored/incoming challenge diagnostic in Development environment only
                var storedChallenge = _webauthnService.GetStoredChallenge(userId.ToString());
                var includeDiagnostics = (_config["ASPNETCORE_ENVIRONMENT"] ?? "Production").ToLowerInvariant() == "development";
                if (includeDiagnostics)
                {
                    return BadRequest(new { success = false, reason = "verification failed", storedChallenge, incomingChallenge = challenge });
                }
                return BadRequest(new { success = false, reason = "verification failed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RegisterVerify exception for user {UserId}", userId);
                return BadRequest(new { success = false, reason = ex.Message });
            }
        }

        [HttpPost("login/options")]
        public async Task<IActionResult> LoginOptions([FromBody] LoginOptionsRequest req)
        {
            // Accept either userId or email
            Domain.Entities.Users.User? user = null;
            if (req.UserId.HasValue)
            {
                user = _userRepository.GetById(req.UserId.Value);
            }
            else if (!string.IsNullOrEmpty(req.Email))
            {
                user = _userRepository.GetByEmail(req.Email);
            }

            if (user == null) return BadRequest("User not found");

            var passkeys = await _passkeyRepository.GetByUserIdAsync(user.Id);
            // Send allowed credential IDs as base64url strings; client will convert to buffers
            var allowed = passkeys.Select(p => p.CredentialId).ToList();

            var options = await _webauthnService.GenerateAssertionOptionsAsync(user.Id, allowed);

            return Ok(options);
        }

        [HttpGet("passkeys")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> GetPasskeys()
        {
            var claim = User.FindFirst("userId")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var userId)) return BadRequest("User not found");

            var passkeys = await _passkeyRepository.GetByUserIdAsync(userId);
            var result = passkeys.Select(p => new { id = p.Id, credentialId = p.CredentialId, createdAt = p.CreatedAt, lastUsedAt = p.LastUsedAt });
            return Ok(result);
        }

        [HttpDelete("passkeys/{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> DeletePasskey([FromRoute] string id)
        {
            var claim = User.FindFirst("userId")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var userId)) return BadRequest("User not found");

            var passkey = await _passkeyRepository.GetByCredentialIdAsync(id);
            if (passkey == null || passkey.UserId != userId) return NotFound();

            await _passkeyRepository.DeleteAsync(passkey.Id);
            return Ok(new { success = true });
        }

        [HttpPost("login/verify")]
        public async Task<IActionResult> LoginVerify([FromBody] VerifyLoginRequest req)
        {
            // Extract credential id from assertion (expect base64url in `id`)
            var assertionJson = System.Text.Json.JsonSerializer.Serialize(req.Assertion);
            using var doc = System.Text.Json.JsonDocument.Parse(assertionJson);
            var root = doc.RootElement;
            if (!root.TryGetProperty("id", out var idElem)) return BadRequest("Invalid assertion");
            var credentialId = idElem.GetString() ?? string.Empty;

            _logger.LogInformation("LoginVerify called for credential {CredId} with challenge {Challenge}", credentialId, req.Challenge);

            var passkey = await _passkeyRepository.GetByCredentialIdAsync(credentialId);
            if (passkey == null) {
                _logger.LogWarning("LoginVerify: passkey not found for credential {CredId}", credentialId);
                return BadRequest(new { success = false, reason = "Passkey not found" });
            }

            // Log stored challenge for user for debugging
            var storedChallenge = _webauthnService.GetStoredChallenge(passkey.UserId.ToString());
            _logger.LogDebug("Stored challenge for user {UserId}: {Stored}", passkey.UserId, storedChallenge);

            // Call service with challenge and userId
            var options = new { challenge = req.Challenge, userId = passkey.UserId.ToString() };
            var ok = await _webauthnService.VerifyAuthenticationAsync(req.Assertion, System.Text.Json.JsonSerializer.SerializeToElement(options), Convert.FromBase64String(passkey.PublicKey), (uint)passkey.Counter);
            _logger.LogInformation("LoginVerify result for credential {CredId}: {Ok}", credentialId, ok);

            if (ok)
            {
                passkey.Counter += 1;
                passkey.LastUsedAt = DateTime.UtcNow;
                await _passkeyRepository.UpdateAsync(passkey);

                var jwtKey = _config["Jwt:Key"] ?? throw new Exception("JWT key missing");
                var user = _userRepository.GetById(passkey.UserId);
                if (user == null) return BadRequest("User not found");

                var token = JwtHelper.GenerateToken(jwtKey, passkey.UserId, user.Email ?? string.Empty);

                // generate and persist refresh token (same approach as password login)
                var refreshToken = Guid.NewGuid().ToString();
                try
                {
                    _redisDb.StringSet($"refresh:{passkey.UserId}", refreshToken, TimeSpan.FromDays(7));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to persist refresh token for user {UserId}", passkey.UserId);
                }

                // return both access and refresh tokens
                return Ok(new { success = true, token, accessToken = token, refreshToken });
            }

            // include diagnostics in development to help debugging
            var stored = _webauthnService.GetStoredChallenge(passkey.UserId.ToString());
            var includeDiagnosticsLogin = (_config["ASPNETCORE_ENVIRONMENT"] ?? "Production").ToLowerInvariant() == "development";
            if (includeDiagnosticsLogin)
            {
                return BadRequest(new { success = false, reason = "verification failed", storedChallenge = stored, incoming = options });
            }

            return BadRequest(new { success = false, reason = "verification failed" });
        }

        private static string Base64UrlEncode(byte[] input)
        {
            var s = Convert.ToBase64String(input);
            s = s.Split('=')[0]; // Remove any trailing '='s
            s = s.Replace('+', '-'); // 62nd char of encoding
            s = s.Replace('/', '_'); // 63rd char of encoding
            return s;
        }

        private static byte[] Base64UrlDecode(string input)
        {
            var s = input;
            s = s.Replace('-', '+');
            s = s.Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            return Convert.FromBase64String(s);
        }
    }
}