using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Application.Interfaces;
using NextShopV2.Domain.Entities.Users;
using NextShopV2.Shared.Extensions;
using NextShopV2.Shared.Helpers;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using System;
using AppApiResponse = NextShopV2.Application.DTOs.Response.ApiResponse;
using AppAuthResponse = NextShopV2.Application.DTOs.Response.AuthResponse;

namespace NextShopV2.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IDatabase _redisDb;
        private readonly string? _jwtKey;

        public AuthService(IUserRepository userRepository, IConnectionMultiplexer redis, IConfiguration config)
        {
            _userRepository = userRepository;
            _redisDb = redis.GetDatabase();
            _jwtKey = config["Jwt:Key"];
        }

    public AppApiResponse Register(RegisterRequest request)
        {
            var passwordHash = PasswordHelper.HashPassword(request.Password!);
            if (_userRepository.ExistsByEmail(request.Email!))
                return new AppApiResponse { Success = false, Message = "Đã tồn tại" };
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email!,
                PasswordHash = passwordHash,
                FullName = request.FullName!,
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };
            _userRepository.Add(user);
            _userRepository.Save();
            return new AppApiResponse { Success = true, Message = "User registered successfully" };
        }

    public AppAuthResponse Login(LoginRequest request)
        {
            var passwordHash = PasswordHelper.HashPassword(request.Password!);
            var user = _userRepository.GetByEmail(request.Email!);
            if (user.IsNull() || user?.PasswordHash != passwordHash)
                return new AppAuthResponse { Success = false, Message = "Invalid credentials" };
            if (string.IsNullOrWhiteSpace(_jwtKey))
                return new AppAuthResponse { Success = false, Message = "JWT key is missing in configuration" };
            
            var accessToken = JwtHelper.GenerateToken(_jwtKey, user.Id, user.Email, user.Role);
            var refreshToken = Guid.NewGuid().ToString();
            _redisDb.StringSet($"refresh:{user.Id}", refreshToken, TimeSpan.FromDays(7));
            return new AppAuthResponse { Success = true, Message = "Login successful", AccessToken = accessToken, RefreshToken = refreshToken };
        }

    public AppAuthResponse Refresh(RefreshTokenRequest request)
        {
            var storedToken = _redisDb.StringGet($"refresh:{request.UserId}");
            if (storedToken != request.RefreshToken)
                return new AppAuthResponse { Success = false, Message = "Invalid refresh token" };
            if (string.IsNullOrWhiteSpace(_jwtKey))
                return new AppAuthResponse { Success = false, Message = "JWT key is missing in configuration" };
            // Include user's role in refreshed token so role-based authorization continues to work
            var user = _userRepository.GetById(request.UserId);
            var role = user?.Role ?? string.Empty;
            var email = user?.Email ?? string.Empty;
            var accessToken = JwtHelper.GenerateToken(_jwtKey, request.UserId, email, role);
            return new AppAuthResponse { Success = true, Message = "Token refreshed", AccessToken = accessToken };
        }

        public AppApiResponse Logout(string accessToken, string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                return new AppApiResponse { Success = false, Message = "Access token is required" };

            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(accessToken);
                var exp = jwt.ValidTo; // UTC
                var now = DateTime.UtcNow;
                if (exp > now)
                {
                    var ttl = exp - now;
                    var key = $"blacklist:access:{accessToken}";
                    _redisDb.StringSet(key, "1", ttl);
                }

                // Invalidate refresh token stored in redis
                // We stored refresh token as "refresh:{userId}" earlier
                var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == "userId");
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    // remove stored refresh token for the user
                    _redisDb.KeyDelete($"refresh:{userId}");
                }

                // Optionally blacklist the refresh token string if provided
                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    // we don't have expiry for refresh token here, set a reasonable TTL (7 days) or remove if stored
                    _redisDb.StringSet($"blacklist:refresh:{refreshToken}", "1", TimeSpan.FromDays(7));
                }

                return new AppApiResponse { Success = true, Message = "Logged out" };
            }
            catch (Exception ex)
            {
                return new AppApiResponse { Success = false, Message = ex.Message };
            }
        }

        public UserResponse? GetMe(Guid userId)
        {
            var user = _userRepository.GetById(userId);
            if (user.IsNull()) 
                return null;

            return new UserResponse
            {
                Id = user!.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
