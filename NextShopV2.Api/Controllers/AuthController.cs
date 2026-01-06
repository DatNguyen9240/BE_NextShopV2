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

        public AuthController(IAuthService authService, Microsoft.Extensions.Logging.ILogger<AuthController> logger, Microsoft.Extensions.Hosting.IHostEnvironment env)
        {
            _authService = authService;
            _logger = logger;
            _env = env;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.BadRequest("Invalid input");
            var result = _authService.Register(request);
            if (!result.Success)
                return ResponseHelper.BadRequest(result.Message ?? string.Empty);
            return ResponseHelper.Success(result.Message ?? string.Empty);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.BadRequest("Invalid input");
            var result = _authService.Login(request);
            
            if (!result.Success)
                return ResponseHelper.Unauthorized(result.Message ?? "Login failed");
            
            return AuthResponseHelper.Success(result.Message ?? string.Empty, result.AccessToken, result.RefreshToken);
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshTokenRequest request)
        {
            var result = _authService.Refresh(request);
            
            if (!result.Success)
                return ResponseHelper.Unauthorized(result.Message ?? "Token refresh failed");
            
            return AuthResponseHelper.Success(result.Message ?? string.Empty, result.AccessToken, result.RefreshToken);
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] NextShopV2.Application.DTOs.Request.LogoutRequest request)
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

            var result = _authService.Logout(accessToken, request?.RefreshToken ?? string.Empty);
            if (!result.Success)
                return ResponseHelper.BadRequest(result.Message ?? "Logout failed");
            return ResponseHelper.Success(result.Message ?? "Logged out");
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetMe()
        {
            // Get userId from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return ResponseHelper.Unauthorized("Invalid token");

            var user = _authService.GetMe(userId);
            if (user is null)
                return ResponseHelper.NotFound("User not found");

            return ResponseHelper.Success(user);
        }

        [HttpPut("me")]
        [Authorize]
        public IActionResult UpdateProfile([FromBody] NextShopV2.Application.DTOs.Request.UpdateProfileRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return ResponseHelper.Unauthorized("Invalid token");

            var result = _authService.UpdateProfile(userId, request);
            if (!result.Success)
                return ResponseHelper.BadRequest(result.Message ?? "Update failed");
            return ResponseHelper.Success(result.Message ?? "Updated");
        }

        [HttpPut("me/address")]
        [Authorize]
        public IActionResult UpsertAddress([FromBody] NextShopV2.Application.DTOs.Request.UpdateAddressRequest request)
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
                var addr = _authService.UpsertAddress(userId, request);
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
        public IActionResult DeleteAddress(Guid addressId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return ResponseHelper.Unauthorized("Invalid token");

            var ok = _authService.DeleteAddress(userId, addressId);
            if (!ok) return ResponseHelper.NotFound("Address not found");
            return ResponseHelper.Success("Deleted");
        }


    }

}
