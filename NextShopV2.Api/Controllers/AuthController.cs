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
            return AuthResponseHelper.Success(result.Message ?? string.Empty, result.AccessToken, result.RefreshToken, result.Data);
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshTokenRequest request)
        {
            var result = _authService.Refresh(request);
            return AuthResponseHelper.Success(result.Message ?? string.Empty, result.AccessToken, result.RefreshToken, result.Data);
        }
    }
}
