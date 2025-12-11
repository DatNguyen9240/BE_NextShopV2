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

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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


    }

}
