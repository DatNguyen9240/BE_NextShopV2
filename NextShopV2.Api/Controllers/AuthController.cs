using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextShopV2.Infrastructure.Persistence;
using NextShopV2.Domain.Entities;
using NextShopV2.Application.Common;
using StackExchange.Redis;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Api.Helpers;
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

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetMe()
        {
            // Get userId from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return ResponseHelper.Unauthorized("Invalid token");

            var user = _authService.GetMe(userId);
            if (user == null)
                return ResponseHelper.NotFound("User not found");

            return ResponseHelper.Success(user);
        }


    }

}
