using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextShopV2.Infrastructure.Persistence;
using NextShopV2.Domain.Entities;
using NextShopV2.Application.Common;
using StackExchange.Redis;
using NextShopV2.Api.Models;
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
            var passwordHash = PasswordHelper.HashPassword(request.Password ?? string.Empty);

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
            var passwordHash = PasswordHelper.HashPassword(request.Password ?? string.Empty);

            var users = _context.Users
                .FromSqlRaw("EXEC dbo.CheckUserLogin @p0, @p1", request.Email, passwordHash)
                .AsEnumerable();

            var user = users.FirstOrDefault();

            if (user == null)
                return ResponseHelper.Unauthorized("Invalid credentials");

            _redisDb.StringSet($"login:{request.Email}", "success", TimeSpan.FromHours(1));

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtKey ?? string.Empty);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            var userData = new { user.Id, user.Email };

            return AuthResponseHelper.Success("Login successful", jwtToken, userData);
        }
    }
}
