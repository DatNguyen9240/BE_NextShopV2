using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextShopV2.Infrastructure.Persistence;
using NextShopV2.Domain.Entities;
using StackExchange.Redis;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Shared.Helpers;
using NextShopV2.Shared.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using NextShopV2.Domain.Entities.Users;
using NextShopV2.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
    private readonly IAuthService _authService;
    private readonly Microsoft.Extensions.Logging.ILogger<AuthController> _logger;
    private readonly Microsoft.Extensions.Hosting.IHostEnvironment _env;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _config;

        public AuthController(IAuthService authService, Microsoft.Extensions.Logging.ILogger<AuthController> logger, Microsoft.Extensions.Hosting.IHostEnvironment env, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _authService = authService;
            _logger = logger;
            _env = env;
            _config = config;
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return ResponseHelper.BadRequest("Token is required");

            var result = await _authService.VerifyEmailToken(token);
            var frontendBase = GetFrontendBaseUrl();
            if (!result.Success)
            {
                var failedUrl = $"{frontendBase.TrimEnd('/')}/auth/verify?status=failed";
                return Redirect(failedUrl);
            }

            var redirectUrl = $"{frontendBase.TrimEnd('/')}/auth/verified?accessToken={System.Net.WebUtility.UrlEncode(result.AccessToken)}&refreshToken={System.Net.WebUtility.UrlEncode(result.RefreshToken)}";
            return Redirect(redirectUrl);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.BadRequest("Invalid input");
            var result = await _authService.Register(request);
            if (!result.Success)
                return ResponseHelper.BadRequest(result.Message ?? string.Empty);
            return ResponseHelper.Success(result.Message ?? string.Empty);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.BadRequest("Invalid input");
            var result = await _authService.Login(request);
            
            if (!result.Success)
                return ResponseHelper.Unauthorized(result.Message ?? "Login failed");
            
            return AuthResponseHelper.Success(result.Message ?? string.Empty, result.AccessToken, result.RefreshToken);
        }

        [HttpPost("google")]
        public async Task<IActionResult> Google([FromBody] NextShopV2.Application.DTOs.Request.GoogleLoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.IdToken))
                return ResponseHelper.BadRequest("Invalid input");

            var result = await _authService.GoogleSignIn(request.IdToken);
            if (!result.Success)
                return ResponseHelper.Unauthorized(result.Message ?? "Google sign-in failed");

            // If tokens are present, return auth response; otherwise indicate verification sent
            if (!string.IsNullOrWhiteSpace(result.AccessToken))
            {
                return AuthResponseHelper.Success(result.Message ?? string.Empty, result.AccessToken, result.RefreshToken);
            }

            return ResponseHelper.Success(new { verificationSent = true }, result.Message ?? string.Empty);
        }
        [HttpPost("google/signup")]
        public async Task<IActionResult> GoogleSignup([FromBody] NextShopV2.Application.DTOs.Request.GoogleLoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.IdToken))
                return ResponseHelper.BadRequest("Invalid input");

            var result = await _authService.GoogleRegister(request.IdToken);
            if (!result.Success)
                return ResponseHelper.Unauthorized(result.Message ?? "Google sign-up failed");

            // If tokens are present, return auth response; otherwise indicate verification sent
            if (!string.IsNullOrWhiteSpace(result.AccessToken))
            {
                return AuthResponseHelper.Success(result.Message ?? string.Empty, result.AccessToken, result.RefreshToken);
            }

            return ResponseHelper.Success(new { verificationSent = true }, result.Message ?? string.Empty);
        }

        [HttpPost("login/start")]
        public async Task<IActionResult> LoginStart([FromBody] NextShopV2.Application.DTOs.Request.StartEmailOtpRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.BadRequest("Invalid input");

            var result = await _authService.StartEmailOtp(request);
            if (!result.Success)
                return ResponseHelper.BadRequest(result.Message ?? "Failed to start MFA");

            // If tokens are returned directly in Data (no MFA), unwrap and return as auth response
            if (result.Data != null && result.Data is object)
            {
                try
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(result.Data);
                    var doc = System.Text.Json.JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("accessToken", out var at))
                    {
                        var access = at.GetString();
                        var refresh = root.TryGetProperty("refreshToken", out var rt) ? rt.GetString() : null;
                        return AuthResponseHelper.Success(result.Message ?? string.Empty, access, refresh);
                    }
                    if (root.TryGetProperty("requestId", out var rid))
                    {
                        return ResponseHelper.Success(new { mfaRequired = true, requestId = rid.GetString() });
                    }
                }
                catch { /* ignore parsing errors and fall through */ }
            }

            return ResponseHelper.Success(new { mfaRequired = true });
        }

        [HttpPost("login/verify")]
        public async Task<IActionResult> LoginVerify([FromBody] NextShopV2.Application.DTOs.Request.VerifyEmailOtpRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.BadRequest("Invalid input");

            var result = await _authService.VerifyEmailOtp(request);
            if (!result.Success)
                return ResponseHelper.Unauthorized(result.Message ?? "Verification failed");

            return AuthResponseHelper.Success(result.Message ?? string.Empty, result.AccessToken, result.RefreshToken);
        }

        [HttpPost("mfa/enable/start")]
        [Authorize]
        public async Task<IActionResult> StartEnableMfa()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return ResponseHelper.Unauthorized("Invalid token");

            var result = await _authService.StartEnableEmailMfa(userId);
            if (!result.Success) return ResponseHelper.BadRequest(result.Message ?? "Failed to send confirmation");
            return ResponseHelper.Success(result.Data, result.Message ?? "Confirmation sent");
        }

        [HttpPost("mfa/enable/verify")]
        [Authorize]
        public async Task<IActionResult> VerifyEnableMfa([FromBody] NextShopV2.Application.DTOs.Request.VerifyEmailOtpRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return ResponseHelper.Unauthorized("Invalid token");

            var result = await _authService.VerifyEnableEmailMfa(request, userId);
            if (!result.Success) return ResponseHelper.BadRequest(result.Message ?? "Verification failed");
            return ResponseHelper.Success(null, result.Message ?? "MFA enabled");
        }

        [HttpPost("mfa/disable")]
        [Authorize]
        public async Task<IActionResult> DisableMfa()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return ResponseHelper.Unauthorized("Invalid token");

            var result = await _authService.DisableEmailMfa(userId);
            if (!result.Success) return ResponseHelper.BadRequest(result.Message ?? "Disable failed");
            return ResponseHelper.Success(null, result.Message ?? "MFA disabled");
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.Refresh(request);
            
            if (!result.Success)
                return ResponseHelper.Unauthorized(result.Message ?? "Token refresh failed");
            
            return AuthResponseHelper.Success(result.Message ?? string.Empty, result.AccessToken, result.RefreshToken);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] NextShopV2.Application.DTOs.Request.LogoutRequest request)
        {
            // Prefer access token from Authorization header: "Bearer <token>", fallback to body.AccessToken
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            string? accessToken = null;
            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer "))
            {
                accessToken = authHeader.Substring("Bearer ".Length).Trim();
            }
            else if (!string.IsNullOrWhiteSpace(request?.AccessToken))
            {
                accessToken = request!.AccessToken;
            }

            if (string.IsNullOrWhiteSpace(accessToken))
                return ResponseHelper.BadRequest("Access token is required either in Authorization header or request body");

            var result = await _authService.Logout(accessToken, request?.RefreshToken ?? string.Empty);
            if (!result.Success)
                return ResponseHelper.BadRequest(result.Message ?? "Logout failed");
            return ResponseHelper.Success(result.Message ?? "Logged out");
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            // Get userId from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return ResponseHelper.Unauthorized("Invalid token");

            var user = await _authService.GetMe(userId);
            if (user is null)
                return ResponseHelper.NotFound("User not found");

            return ResponseHelper.Success(user);
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] NextShopV2.Application.DTOs.Request.UpdateProfileRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return ResponseHelper.Unauthorized("Invalid token");

            var result = await _authService.UpdateProfile(userId, request);
            if (!result.Success)
                return ResponseHelper.BadRequest(result.Message ?? "Update failed");
            return ResponseHelper.Success(result.Message ?? "Updated");
        }

        [HttpPut("me/address")]
        [Authorize]
        public async Task<IActionResult> UpsertAddress([FromBody] NextShopV2.Application.DTOs.Request.UpdateAddressRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return ResponseHelper.Unauthorized("Invalid token");

            // Defensive: ensure we have a request body (model binding may return null in some cases)
            if (request == null)
                return ResponseHelper.BadRequest("Request body is required and must be valid JSON");

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.FullAddress))
                return ResponseHelper.BadRequest("FullAddress is required");

            // If request.AddressId is present but not a GUID, we treat it as "create new address" (client may send place_id)
            try
            {
                var addr = await _authService.UpsertAddress(userId, request);
                if (addr is null)
                    return ResponseHelper.NotFound("User not found");

                return ResponseHelper.Success(addr);
            }
            catch (Exception ex)
            {
                // Log the exception for investigation
                _logger.LogError(ex, "UpsertAddress failed for user {UserId}", userId);

                // If this is an explicit concurrency failure surfaced by the service, return 409 Conflict
                if (ex is InvalidOperationException && ex.Message != null && ex.Message.StartsWith("Concurrency conflict"))
                {
                    return ResponseHelper.Conflict(ex.Message);
                }

                // In development provide detailed message to help debugging; otherwise return generic error
                if (_env.IsDevelopment())
                    return ResponseHelper.InternalServerError(ex.Message ?? "Internal server error");

                return ResponseHelper.InternalServerError("An unexpected error occurred");
            }
        }

        [HttpDelete("me/address/{addressId}")]
        [Authorize]
        public async Task<IActionResult> DeleteAddress(Guid addressId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return ResponseHelper.Unauthorized("Invalid token");

            var ok = await _authService.DeleteAddress(userId, addressId);
            if (!ok) return ResponseHelper.NotFound("Address not found");
            return ResponseHelper.Success("Deleted");
        }

        [HttpPost("me/deactivate")]
        [Authorize]
        public async Task<IActionResult> DeactivateAccount()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return ResponseHelper.Unauthorized("Invalid token");

            // Attempt to get access token from Authorization header so we can blacklist it as well
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            string? accessToken = null;
            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer "))
            {
                accessToken = authHeader.Substring("Bearer ".Length).Trim();
            }

            var result = await _authService.DeactivateAccount(userId);
            if (!result.Success) return ResponseHelper.BadRequest(result.Message ?? "Failed to deactivate");

            // Blacklist the access token if present
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                await _authService.Logout(accessToken, string.Empty);
            }

            return ResponseHelper.Success(result.Message ?? "Account deactivated");
        }

        // ADMIN: Issue welcome voucher to a specific user (idempotent)
        [HttpPost("admin/issue-welcome/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> IssueWelcomeToUser(Guid userId)
        {
            var result = await _authService.IssueWelcomeVoucher(userId);
            if (!result.Success) return ResponseHelper.BadRequest(result.Message ?? "Issue failed");
            return ResponseHelper.Success(result.Message ?? "Issued");
        }

        // ADMIN: Issue welcome vouchers to all users (idempotent)
        [HttpPost("admin/issue-welcome/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> IssueWelcomeToAll()
        {
            var result = await _authService.IssueWelcomeVoucherToAll();
            if (!result.Success) return ResponseHelper.BadRequest(result.Message ?? "Issue failed");
            return ResponseHelper.Success(result.Message ?? "Issued to all (where eligible)");
        }

        // ADMIN: Issue welcome voucher by email
        public class IssueByEmailRequest { public string Email { get; set; } }

        [HttpPost("admin/issue-welcome-by-email")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> IssueWelcomeToUserByEmail([FromBody] IssueByEmailRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email)) return ResponseHelper.BadRequest("Email is required");
            var result = await _authService.IssueWelcomeVoucherByEmail(request.Email);
            if (!result.Success) return ResponseHelper.BadRequest(result.Message ?? "Issue failed");
            return ResponseHelper.Success(result.Message ?? "Issued");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.BadRequest("Invalid input");

            var result = await _authService.ForgotPassword(request);
            if (!result.Success)
                return ResponseHelper.BadRequest(result.Message ?? "Failed to process request");

            return ResponseHelper.Success(result.Message ?? "Password reset link sent");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.BadRequest("Invalid input");

            var result = await _authService.ResetPassword(request);
            if (!result.Success)
                return ResponseHelper.BadRequest(result.Message ?? "Failed to reset password");

            return ResponseHelper.Success(result.Message ?? "Password reset successfully");
        }

        private string GetFrontendBaseUrl()
        {
            var configValue = _config["Frontend:BaseUrl"];
            
            // Check if config value is a placeholder that wasn't expanded
            if (string.IsNullOrWhiteSpace(configValue) || configValue.StartsWith("${"))
            {
                // Try to get from environment variable
                var envValue = Environment.GetEnvironmentVariable("FRONTEND_URL");
                if (!string.IsNullOrWhiteSpace(envValue))
                    return envValue;
            }
            
            // Return config value or default
            return configValue ?? "http://localhost:3000";
        }

    }

}
