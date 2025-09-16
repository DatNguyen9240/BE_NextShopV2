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

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDatabase _redisDb;
        private readonly string? _jwtKey;

        public AuthController(AppDbContext context, IConnectionMultiplexer redis, IConfiguration config)
        {
            _context = context;
            _redisDb = redis.GetDatabase();
            _jwtKey = config["Jwt:Key"];
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.BadRequest("Invalid input");

            var passwordHash = PasswordHelper.HashPassword(request.Password!);

            try
            {
                _context.Database.ExecuteSqlRaw("EXEC dbo.RegisterUser @p0, @p1", request.Email, passwordHash);
                return ResponseHelper.Success("User registered successfully");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("User already exists"))
                    return ResponseHelper.BadRequest("User already exists");
                return ResponseHelper.ServerError("Registration failed");
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.BadRequest("Invalid input");

            var passwordHash = PasswordHelper.HashPassword(request.Password!);

            var users = _context.Users
                .FromSqlRaw("EXEC dbo.CheckUserLogin @p0, @p1", request.Email, passwordHash)
                .AsEnumerable();

            var user = users.FirstOrDefault();

            if (user == null)
                return ResponseHelper.Unauthorized("Invalid credentials");

            if (string.IsNullOrWhiteSpace(_jwtKey))
                return AuthResponseHelper.ServerError("JWT key is missing in configuration");
            var accessToken = JwtHelper.GenerateToken(_jwtKey, user.Id, user.Email);

            // Sinh refresh token (random string)
            var refreshToken = Guid.NewGuid().ToString();

            // Lưu refresh token vào Redis với key là userId
            _redisDb.StringSet($"refresh:{user.Id}", refreshToken, TimeSpan.FromDays(7));

            var userData = new { user.Id, user.Email };

            return AuthResponseHelper.Success("Login successful", accessToken, refreshToken, new { User = userData });
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshTokenRequest request)
        {
            var storedToken = _redisDb.StringGet($"refresh:{request.UserId}");
            if (storedToken != request.RefreshToken)
                return AuthResponseHelper.Unauthorized("Invalid refresh token");

            if (string.IsNullOrWhiteSpace(_jwtKey))
                return AuthResponseHelper.ServerError("JWT key is missing in configuration");
            var accessToken = JwtHelper.GenerateToken(_jwtKey, request.UserId, "");

            return AuthResponseHelper.Success("Token refreshed", accessToken, null);
        }
    }
}
