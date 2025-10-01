using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Application.Interfaces;
using NextShopV2.Domain.Entities.Users;
using NextShopV2.Application.Common;
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
            
            var accessToken = JwtHelper.GenerateToken(_jwtKey, user.Id, user.Email);
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
            var accessToken = JwtHelper.GenerateToken(_jwtKey, request.UserId, "");
            return new AppAuthResponse { Success = true, Message = "Token refreshed", AccessToken = accessToken };
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
