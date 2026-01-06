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
            if (user == null || user.PasswordHash != passwordHash)
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
            if (user == null) 
                return null; 

            return new UserResponse
            {
                Id = user!.Id,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Gender = user.Gender,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                // Order addresses with default first for nicer UX
                Addresses = user.Addresses
                    .OrderByDescending(a => a.IsDefault)
                    .Select(a => new AddressResponse
                    {
                        AddressId = a.AddressId,
                        FullAddress = a.FullAddress,
                        Latitude = a.Latitude,
                        Longitude = a.Longitude,
                        IsDefault = a.IsDefault
                    }).ToList(),
                // include avatar URL if present (allow null)
                Avatar = user.Avatar
            };
        }

        public AppApiResponse UpdateProfile(Guid userId, UpdateProfileRequest request)
        {
            var user = _userRepository.GetById(userId);
            if (user == null)
                return new AppApiResponse { Success = false, Message = "User not found" }; 

            if (!string.IsNullOrWhiteSpace(request.FullName))
                user.FullName = request.FullName!;
            if (!string.IsNullOrWhiteSpace(request.Phone))
                user.Phone = request.Phone;
            if (!string.IsNullOrWhiteSpace(request.Gender))
                user.Gender = request.Gender;
            // Support explicit clearing of avatar by passing null, or updating when non-empty value provided
            if (request.AvatarUrl == null)
            {
                user.Avatar = null;
            }
            else if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
            {
                user.Avatar = request.AvatarUrl;
            }

            user.UpdatedAt = DateTime.UtcNow;
            _userRepository.Save();

            return new AppApiResponse { Success = true, Message = "Profile updated" };
        }

        public AddressResponse? UpsertAddress(Guid userId, UpdateAddressRequest request)
        {
            var user = _userRepository.GetById(userId);
            if (user == null)
                return null; 

            // Parse incoming AddressId if provided. If it's not a valid GUID, treat as new address (create)
            Guid? parsedAddressId = null;
            if (!string.IsNullOrWhiteSpace(request.AddressId))
            {
                if (Guid.TryParse(request.AddressId, out var g))
                    parsedAddressId = g;
                // else: ignore invalid non-GUID values and create a new address
            }

            // If this address should be default, unset other defaults in DB atomically (exclude current address when updating)
            if (request.IsDefault)
            {
                _userRepository.UnsetDefaultAddresses(userId, parsedAddressId);
            }

            Domain.Entities.Users.Address? address = null;
            if (parsedAddressId.HasValue)
            {
                address = user.Addresses.FirstOrDefault(a => a.AddressId == parsedAddressId.Value);
            }

            if (address == null)
            {
                // Create new address record directly (avoid concurrency with tracked entities)
                var newAddress = new Domain.Entities.Users.Address
                {
                    AddressId = Guid.NewGuid(),
                    UserId = userId,
                    RecipientName = user.FullName,
                    FullAddress = request.FullAddress,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    IsDefault = request.IsDefault
                };

                _userRepository.InsertAddress(newAddress);

                try
                {
                    _userRepository.Save();

                    return new AddressResponse
                    {
                        AddressId = newAddress.AddressId,
                        FullAddress = newAddress.FullAddress,
                        Latitude = newAddress.Latitude,
                        Longitude = newAddress.Longitude,
                        IsDefault = newAddress.IsDefault
                    };
                }
                catch
                {
                    // If insert fails with concurrency (rare), rethrow to be handled upstream
                    throw;
                }
            }
            else
            {
                // Try atomic DB update first
                var updated = _userRepository.TryUpdateAddress(address.AddressId, request.FullAddress, request.Latitude, request.Longitude, request.IsDefault);
                if (updated)
                {
                    return new AddressResponse
                    {
                        AddressId = address.AddressId,
                        FullAddress = request.FullAddress,
                        Latitude = request.Latitude,
                        Longitude = request.Longitude,
                        IsDefault = request.IsDefault
                    };
                }

                // If atomic update didn't affect rows (concurrency or missing), create a new address record instead
                var fallbackAddress = new Domain.Entities.Users.Address
                {
                    AddressId = Guid.NewGuid(),
                    UserId = userId,
                    RecipientName = user.FullName,
                    FullAddress = request.FullAddress,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    IsDefault = request.IsDefault
                };

                _userRepository.InsertAddress(fallbackAddress);

                try
                {
                    _userRepository.Save();

                    return new AddressResponse
                    {
                        AddressId = fallbackAddress.AddressId,
                        FullAddress = fallbackAddress.FullAddress,
                        Latitude = fallbackAddress.Latitude,
                        Longitude = fallbackAddress.Longitude,
                        IsDefault = fallbackAddress.IsDefault
                    };
                }
                catch
                {
                    // If saving still fails (very rare), surface as concurrency error to be handled by the controller
                    throw new InvalidOperationException("Concurrency conflict: Could not save address after retries");
                }
            }

            // This point should never be reached; surface as an error if it does
            throw new InvalidOperationException("Unexpected state in UpsertAddress");
        }

        public bool DeleteAddress(Guid userId, Guid addressId)
        {
            var user = _userRepository.GetById(userId);
            if (user == null) return false;

            var address = user.Addresses.FirstOrDefault(a => a.AddressId == addressId);
            if (address == null) return false;

            var wasDefault = address.IsDefault;

            user.Addresses.Remove(address);

            // If we deleted the default address, pick another address and mark it default
            if (wasDefault && user.Addresses.Any())
            {
                // Prefer an address that was previously not default; pick the first one
                var next = user.Addresses.First();
                next.IsDefault = true;
            }

            _userRepository.Save();
            return true;
        }
    }
}
