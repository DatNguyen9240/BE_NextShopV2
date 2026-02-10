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
        private readonly NextShopV2.Application.Interfaces.Services.IEmailService _emailService;
        private readonly string? _mfaKey;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;

        public AuthService(IUserRepository userRepository, IConnectionMultiplexer redis, IConfiguration config, NextShopV2.Application.Interfaces.Services.IEmailService emailService)
        {
            _userRepository = userRepository;
            _redisDb = redis.GetDatabase();
            _jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? config["Jwt:Key"];
            _emailService = emailService;
            _mfaKey = config["Mfa:Key"] ?? config["Jwt:Key"];
            _config = config;
        }

    public async Task<AppApiResponse> Register(RegisterRequest request)
        {
            // check existence first to respond immediately if email taken
            var existingUser = await _userRepository.GetByEmailAsync(request.Email!);
            if (existingUser != null)
                return new AppApiResponse { Success = false, Message = "Đã tồn tại" };

            var passwordHash = PasswordHelper.HashPassword(request.Password!);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email!,
                PasswordHash = passwordHash,
                FullName = request.FullName!,
                Role = "User",
                EmailVerified = false,
                CreatedAt = DateTime.UtcNow
            };
            _userRepository.Add(user);
            await _userRepository.SaveAsync();

            // Send verification email after successful registration
            var sent = await StartEmailVerification(user.Id, user.Email);
            if (!sent.Success)
            {
                return new AppApiResponse { Success = true, Message = "Đăng ký thành công nhưng gửi email xác thực thất bại" };
            }

            return new AppApiResponse { Success = true, Message = "Đăng ký thành công. Đã gửi email xác thực" };
        }

    public async Task<AppAuthResponse> Login(LoginRequest request)
        {
            var passwordHash = PasswordHelper.HashPassword(request.Password!);
            var user = await _userRepository.GetByEmailAsync(request.Email!);
            if (user == null || user.PasswordHash != passwordHash)
                return new AppAuthResponse { Success = false, Message = "Thông tin đăng nhập không hợp lệ" };

            // Require email verification before issuing tokens
            if (!user.EmailVerified)
            {
                // Send verification email
                await StartEmailVerification(user.Id, user.Email);
                return new AppAuthResponse { Success = false, Message = "Email chưa được xác thực. Đã gửi email xác thực" };
            }

            if (string.IsNullOrWhiteSpace(_jwtKey))
                return new AppAuthResponse { Success = false, Message = "JWT key is missing in configuration" };
            var accessToken = JwtHelper.GenerateToken(_jwtKey, user.Id, user.Email, user.Role);
            var refreshToken = Guid.NewGuid().ToString();
            await _redisDb.StringSetAsync($"refresh:{user.Id}", refreshToken, TimeSpan.FromDays(7));
            return new AppAuthResponse { Success = true, Message = "Login successful", AccessToken = accessToken, RefreshToken = refreshToken };
        }

        // --- Email OTP (MFA) ---
        public async Task<AppApiResponse> StartEmailOtp(NextShopV2.Application.DTOs.Request.StartEmailOtpRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return new AppApiResponse { Success = false, Message = "Dữ liệu không hợp lệ" };

            var passwordHash = PasswordHelper.HashPassword(request.Password!);
            var user = await _userRepository.GetByEmailAsync(request.Email!);
            if (user == null || user.PasswordHash != passwordHash)
                return new AppApiResponse { Success = false, Message = "Invalid credentials" };

            // For users without MFA enabled, return tokens directly (backwards compatible)
            if (!user.MfaEnabled || string.IsNullOrWhiteSpace(user.MfaType) || !user.MfaType.Equals("Email", System.StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(_jwtKey))
                    return new AppApiResponse { Success = false, Message = "JWT key is missing in configuration" };
                var accessToken = JwtHelper.GenerateToken(_jwtKey, user.Id, user.Email, user.Role);
                var refreshToken = Guid.NewGuid().ToString();
                await _redisDb.StringSetAsync($"refresh:{user.Id}", refreshToken, TimeSpan.FromDays(7));
                return new AppApiResponse { Success = true, Message = "Login successful", Data = new { accessToken, refreshToken } };
            }

            // Create OTP code
            var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            // 6-digit code (000000 - 999999)
            var code = (System.BitConverter.ToUInt32(bytes, 0) % 1000000).ToString("D6");

            // Compute hash of code with secret + userId
            var codeHash = ComputeCodeHash(code, user.Id);
            var requestId = Guid.NewGuid().ToString();

            var payload = new {
                userId = user.Id,
                codeHash = codeHash,
                attempts = 5
            };
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            await _redisDb.StringSetAsync($"mfa:email:req:{requestId}", json, TimeSpan.FromMinutes(5));

            // Send email (await here to surface errors)
            var subject = "Emxinh.shop: Mã xác thực của bạn";
            var html = LoadOtpTemplate(code, "Đăng nhập", 5);
            try
            {
                await _emailService.SendEmailAsync(user.Email, subject, html);
            }
            catch (System.Exception)
            {
                // remove the stored request on email failure to avoid orphaned OTPs
                await _redisDb.KeyDeleteAsync($"mfa:email:req:{requestId}");
                return new AppApiResponse { Success = false, Message = "Gửi email OTP thất bại" };
            }

            return new AppApiResponse { Success = true, Message = "Yêu cầu xác thực 2 lớp", Data = new { requestId } };
        }

        public async Task<AppAuthResponse> VerifyEmailOtp(NextShopV2.Application.DTOs.Request.VerifyEmailOtpRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RequestId) || string.IsNullOrWhiteSpace(request.Code))
                return new AppAuthResponse { Success = false, Message = "Invalid input" };

            var key = $"mfa:email:req:{request.RequestId}";
            var json = await _redisDb.StringGetAsync(key);
            if (json.IsNullOrEmpty)
                return new AppAuthResponse { Success = false, Message = "Invalid or expired request" };

            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json.ToString()!);
                var root = doc.RootElement;
                var userId = root.GetProperty("userId").GetGuid();
                var codeHash = root.GetProperty("codeHash").GetString();
                var attempts = root.GetProperty("attempts").GetInt32();

                if (attempts <= 0)
                {
                    await _redisDb.KeyDeleteAsync(key);
                    return new AppAuthResponse { Success = false, Message = "Quá nhiều lần thử" };
                }

                var providedHash = ComputeCodeHash(request.Code!, userId);
                if (!string.Equals(providedHash, codeHash))
                {
                    // decrement attempts and update store (keep same TTL)
                    var newAttempts = attempts - 1;
                    var updated = new { userId = userId, codeHash = codeHash, attempts = newAttempts };
                    var ttl = await _redisDb.KeyTimeToLiveAsync(key) ?? TimeSpan.FromMinutes(5);
                    await _redisDb.StringSetAsync(key, System.Text.Json.JsonSerializer.Serialize(updated), ttl);
                    return new AppAuthResponse { Success = false, Message = "Mã không hợp lệ" };
                }

                // success: remove request and issue tokens
                await _redisDb.KeyDeleteAsync(key);
                if (string.IsNullOrWhiteSpace(_jwtKey))
                    return new AppAuthResponse { Success = false, Message = "JWT key is missing in configuration" };
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return new AppAuthResponse { Success = false, Message = "User not found" };
                var accessToken = JwtHelper.GenerateToken(_jwtKey, user.Id, user.Email, user.Role);
                var refreshToken = Guid.NewGuid().ToString();
                await _redisDb.StringSetAsync($"refresh:{user.Id}", refreshToken, TimeSpan.FromDays(7));
                return new AppAuthResponse { Success = true, Message = "Login successful", AccessToken = accessToken, RefreshToken = refreshToken };
            }
            catch (System.Exception)
            {
                return new AppAuthResponse { Success = false, Message = "Dữ liệu yêu cầu không hợp lệ" };
            }
        }

        private string ComputeCodeHash(string code, System.Guid userId)
        {
            var combined = System.Text.Encoding.UTF8.GetBytes(code + userId.ToString() + (_mfaKey ?? string.Empty));
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hash = sha.ComputeHash(combined);
            return System.Convert.ToBase64String(hash);
        }

        private string LoadOtpTemplate(string code, string purpose, int expiryMinutes)
        {
            try
            {
                // Attempt to read template from the output folder where the file was copied
                var baseDir = System.AppContext.BaseDirectory ?? ".";
                var path = System.IO.Path.Combine(baseDir, "EmailTemplates", "otp_email_template.html");
                string template = System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path) : null!;
                if (string.IsNullOrWhiteSpace(template))
                {
                    // fallback simple HTML
                    template = $"<p>{purpose}: <strong>{code}</strong> (expires in {expiryMinutes} minutes)</p>";
                }

                template = template.Replace("{{code}}", System.Net.WebUtility.HtmlEncode(code));
                template = template.Replace("{{siteName}}", "Emxinh.shop");
                template = template.Replace("{{expiryMinutes}}", expiryMinutes.ToString());
                return template;
            }
            catch
            {
                return $"<p>{purpose}: <strong>{code}</strong> (expires in {expiryMinutes} minutes)</p>";
            }
        }

        public async Task<AppApiResponse> StartEnableEmailMfa(System.Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return new AppApiResponse { Success = false, Message = "User not found" };

            // Generate code
            var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var code = (System.BitConverter.ToUInt32(bytes, 0) % 1000000).ToString("D6");
            var codeHash = ComputeCodeHash(code, user.Id);
            var requestId = System.Guid.NewGuid().ToString();

            var payload = new {
                userId = user.Id,
                codeHash = codeHash,
                attempts = 5
            };
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            await _redisDb.StringSetAsync($"mfa:enable:req:{requestId}", json, TimeSpan.FromMinutes(10));

            var subject = "Emxinh.shop: Xác nhận bật 2 lớp";
            var html = LoadOtpTemplate(code, "Bật xác thực 2 lớp", 10);
            try
            {
                await _emailService.SendEmailAsync(user.Email, subject, html);
            }
            catch (System.Exception)
            {
                await _redisDb.KeyDeleteAsync($"mfa:enable:req:{requestId}");
                return new AppApiResponse { Success = false, Message = "Failed to send confirmation email" };
            }

            return new AppApiResponse { Success = true, Message = "Confirmation email sent", Data = new { requestId } };
        }

        public async Task<AppApiResponse> VerifyEnableEmailMfa(NextShopV2.Application.DTOs.Request.VerifyEmailOtpRequest request, System.Guid userId)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RequestId) || string.IsNullOrWhiteSpace(request.Code))
                return new AppApiResponse { Success = false, Message = "Invalid input" };

            var key = $"mfa:enable:req:{request.RequestId}";
            var json = await _redisDb.StringGetAsync(key);
            if (json.IsNullOrEmpty) return new AppApiResponse { Success = false, Message = "Invalid or expired request" };

            using var doc = System.Text.Json.JsonDocument.Parse(json.ToString()!);
            var root = doc.RootElement;
            var storedUserId = root.GetProperty("userId").GetGuid();
            var codeHash = root.GetProperty("codeHash").GetString();
            var attempts = root.GetProperty("attempts").GetInt32();

            if (storedUserId != userId) return new AppApiResponse { Success = false, Message = "Invalid request" };
            if (attempts <= 0) { await _redisDb.KeyDeleteAsync(key); return new AppApiResponse { Success = false, Message = "Too many attempts" }; }

            var providedHash = ComputeCodeHash(request.Code!, storedUserId);
            if (!string.Equals(providedHash, codeHash))
            {
                var newAttempts = attempts - 1;
                var updated = new { userId = storedUserId, codeHash = codeHash, attempts = newAttempts };
                var ttl = await _redisDb.KeyTimeToLiveAsync(key) ?? TimeSpan.FromMinutes(10);
                await _redisDb.StringSetAsync(key, System.Text.Json.JsonSerializer.Serialize(updated), ttl);
                return new AppApiResponse { Success = false, Message = "Invalid code" };
            }

            // success
            await _redisDb.KeyDeleteAsync(key);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return new AppApiResponse { Success = false, Message = "User not found" };
            user.MfaEnabled = true;
            user.MfaType = "Email";
            await _userRepository.SaveAsync();
            return new AppApiResponse { Success = true, Message = "Đã bật xác thực 2 lớp" };
        }

        public async Task<AppApiResponse> DisableEmailMfa(System.Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return new AppApiResponse { Success = false, Message = "User not found" };
            user.MfaEnabled = false;
            user.MfaType = null;
            await _userRepository.SaveAsync();
            return new AppApiResponse { Success = true, Message = "Đã tắt xác thực 2 lớp" };
        }

        // --- Email verification flows ---
        public async Task<AppApiResponse> StartEmailVerification(System.Guid userId, string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return new AppApiResponse { Success = false, Message = "Email không hợp lệ" };
            var token = System.Guid.NewGuid().ToString();
            var key = $"verify:email:{token}";
            await _redisDb.StringSetAsync(key, userId.ToString(), TimeSpan.FromHours(24));

            var frontendBase = GetFrontendBaseUrl();
            var verifyUrl = $"{frontendBase.TrimEnd('/')}/auth/verify?token={token}";
            var subject = "Emxinh.shop: Xác nhận email của bạn";
            var html = LoadVerifyTemplate(verifyUrl, 24);

            try
            {
                await _emailService.SendEmailAsync(email, subject, html);
            }
            catch (System.Exception)
            {
                await _redisDb.KeyDeleteAsync(key);
                return new AppApiResponse { Success = false, Message = "Gửi email xác thực thất bại" };
            }

            return new AppApiResponse { Success = true, Message = "Đã gửi email xác thực" };
        }

        public async Task<AppAuthResponse> VerifyEmailToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return new AppAuthResponse { Success = false, Message = "Invalid token" };
            var key = $"verify:email:{token}";
            var val = await _redisDb.StringGetAsync(key);
            if (val.IsNullOrEmpty) return new AppAuthResponse { Success = false, Message = "Token không hợp lệ hoặc đã hết hạn" };

            if (!System.Guid.TryParse(val.ToString(), out var userId)) return new AppAuthResponse { Success = false, Message = "Dữ liệu token không hợp lệ" };
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return new AppAuthResponse { Success = false, Message = "User not found" };

            user.EmailVerified = true;
            await _userRepository.SaveAsync();
            await _redisDb.KeyDeleteAsync(key);

            if (string.IsNullOrWhiteSpace(_jwtKey)) return new AppAuthResponse { Success = false, Message = "JWT key is missing in configuration" };
            var accessToken = JwtHelper.GenerateToken(_jwtKey, user.Id, user.Email, user.Role);
            var refreshToken = System.Guid.NewGuid().ToString();
            await _redisDb.StringSetAsync($"refresh:{user.Id}", refreshToken, TimeSpan.FromDays(7));
            return new AppAuthResponse { Success = true, Message = "Xác thực email thành công", AccessToken = accessToken, RefreshToken = refreshToken };
        }

        // Sign-in using Google: do not create a new account. If user exists and verified, issue tokens; if exists but unverified, send verification; if not exists, return not found.
        public async Task<AppAuthResponse> GoogleSignIn(string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken)) return new AppAuthResponse { Success = false, Message = "Invalid token" };
            try
            {
                var payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(idToken);
                var email = payload.Email ?? string.Empty;
                var googleId = payload.Subject ?? string.Empty;

                if (string.IsNullOrWhiteSpace(email))
                    return new AppAuthResponse { Success = false, Message = "Google token did not contain email" };

                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    return new AppAuthResponse { Success = false, Message = "User not found" };
                }

                // link google id if not present
                if (string.IsNullOrWhiteSpace(user.GoogleId)) user.GoogleId = googleId;
                await _userRepository.SaveAsync();

                // Require verification before issuing tokens
                if (!user.EmailVerified)
                {
                    // Do not auto-send verification on sign-in; return an error so client can prompt the user to register or request verification
                    return new AppAuthResponse { Success = false, Message = "Email not verified" };
                }

                if (string.IsNullOrWhiteSpace(_jwtKey)) return new AppAuthResponse { Success = false, Message = "JWT key is missing in configuration" };
                var accessToken = JwtHelper.GenerateToken(_jwtKey, user.Id, user.Email, user.Role);
                var refreshToken = Guid.NewGuid().ToString();
                await _redisDb.StringSetAsync($"refresh:{user.Id}", refreshToken, TimeSpan.FromDays(7));
                return new AppAuthResponse { Success = true, Message = "Login successful", AccessToken = accessToken, RefreshToken = refreshToken };
            }
            catch (System.Exception)
            {
                return new AppAuthResponse { Success = false, Message = "Invalid Google token" };
            }
        }

        // Register using Google: create a new user (unverified) and send verification email
        public async Task<AppAuthResponse> GoogleRegister(string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken)) return new AppAuthResponse { Success = false, Message = "Invalid token" };
            try
            {
                var payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(idToken);
                var email = payload.Email ?? string.Empty;
                var googleId = payload.Subject ?? string.Empty;

                if (string.IsNullOrWhiteSpace(email))
                    return new AppAuthResponse { Success = false, Message = "Google token did not contain email" };

                var existing = await _userRepository.GetByEmailAsync(email);
                if (existing != null)
                {
                    // If user already exists, behave like sign-in attempt
                    return await GoogleSignIn(idToken);
                }

                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    FullName = payload.Name ?? string.Empty,
                    PasswordHash = PasswordHelper.HashPassword(Guid.NewGuid().ToString()),
                    Role = "User",
                    GoogleId = googleId,
                    EmailVerified = false,
                    CreatedAt = DateTime.UtcNow
                };
                _userRepository.Add(newUser);
                await _userRepository.SaveAsync();

                await StartEmailVerification(newUser.Id, newUser.Email);
                return new AppAuthResponse { Success = true, Message = "Đã gửi email xác thực" };
            }
            catch (System.Exception)
            {
                return new AppAuthResponse { Success = false, Message = "Invalid Google token" };
            }
        }

        private string LoadVerifyTemplate(string verifyUrl, int expiryHours)
        {
            try
            {
                var baseDir = System.AppContext.BaseDirectory ?? ".";
                var path = System.IO.Path.Combine(baseDir, "EmailTemplates", "verify_email_template.html");
                string template = System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path) : null!;
                if (string.IsNullOrWhiteSpace(template))
                {
                    template = $"<p>Please verify your email by clicking the link: <a href=\"{System.Net.WebUtility.HtmlEncode(verifyUrl)}\">Verify</a></p>";
                }

                template = template.Replace("{{verificationUrl}}", verifyUrl);
                template = template.Replace("{{siteName}}", "Emxinh.shop");
                template = template.Replace("{{expiryHours}}", expiryHours.ToString());
                template = template.Replace("{{buttonText}}", "Xác thực email");
                template = template.Replace("{{year}}", DateTime.UtcNow.Year.ToString());
                return template;
            }
            catch
            {
                return $"<p>Please verify your email by clicking the link: <a href=\"{System.Net.WebUtility.HtmlEncode(verifyUrl)}\">Verify</a></p>";
            }
        }

        public async Task<AppAuthResponse> Refresh(RefreshTokenRequest request)
        {
            var storedToken = await _redisDb.StringGetAsync($"refresh:{request.UserId}");
            if (storedToken != request.RefreshToken)
                return new AppAuthResponse { Success = false, Message = "Refresh token không hợp lệ" };
            if (string.IsNullOrWhiteSpace(_jwtKey))
                return new AppAuthResponse { Success = false, Message = "JWT key is missing in configuration" };
            // Include user's role in refreshed token so role-based authorization continues to work
            var user = await _userRepository.GetByIdAsync(request.UserId);
            var role = user?.Role ?? string.Empty;
            var email = user?.Email ?? string.Empty;
            var accessToken = JwtHelper.GenerateToken(_jwtKey, request.UserId, email, role);
            return new AppAuthResponse { Success = true, Message = "Làm mới token thành công", AccessToken = accessToken };
        }

        public async Task<AppApiResponse> Logout(string accessToken, string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                return new AppApiResponse { Success = false, Message = "Yêu cầu access token" };

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
                    await _redisDb.StringSetAsync(key, "1", ttl);
                }

                // Invalidate refresh token stored in redis
                // We stored refresh token as "refresh:{userId}" earlier
                var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == "userId");
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    // remove stored refresh token for the user
                    await _redisDb.KeyDeleteAsync($"refresh:{userId}");
                }

                // Optionally blacklist the refresh token string if provided
                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    // we don't have expiry for refresh token here, set a reasonable TTL (7 days) or remove if stored
                    await _redisDb.StringSetAsync($"blacklist:refresh:{refreshToken}", "1", TimeSpan.FromDays(7));
                }

                return new AppApiResponse { Success = true, Message = "Đã đăng xuất" };
            }
            catch (Exception ex)
            {
                return new AppApiResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<UserResponse?> GetMe(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
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
                Avatar = user.Avatar,
                MfaEnabled = user.MfaEnabled,
                MfaType = user.MfaType
            };
        }

        public async Task<AppApiResponse> UpdateProfile(Guid userId, UpdateProfileRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new AppApiResponse { Success = false, Message = "User not found" }; 

            if (!string.IsNullOrWhiteSpace(request.FullName))
                user.FullName = request.FullName!;
            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                var existingUser = await _userRepository.GetByPhoneAsync(request.Phone);
                if (existingUser != null && existingUser.Id != userId)
                {
                    return new AppApiResponse { Success = false, Message = "Số điện thoại đã tồn tại" };
                }
                user.Phone = request.Phone;
            }
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
            await _userRepository.SaveAsync();

            return new AppApiResponse { Success = true, Message = "Cập nhật hồ sơ thành công" };
        }

        public async Task<AddressResponse?> UpsertAddress(Guid userId, UpdateAddressRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
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
                    await _userRepository.SaveAsync();

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
                    await _userRepository.SaveAsync();

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

        public async Task<bool> DeleteAddress(Guid userId, Guid addressId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
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

            await _userRepository.SaveAsync();
            return true;
        }

        // --- Forgot Password & Reset Password ---
        public async Task<AppApiResponse> ForgotPassword(ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return new AppApiResponse { Success = false, Message = "Email là bắt buộc" };

            var user = await _userRepository.GetByEmailAsync(request.Email);
            // For security, don't reveal whether the email exists or not
            // Always return success to prevent email enumeration attacks
            if (user == null)
            {
                // Simulate delay to prevent timing attacks
                await Task.Delay(100);
                return new AppApiResponse { Success = true, Message = "Nếu email tồn tại, link reset mật khẩu đã được gửi" };
            }

            // Generate reset token
            var resetToken = Guid.NewGuid().ToString();
            var key = $"reset:password:{resetToken}";
            await _redisDb.StringSetAsync(key, user.Id.ToString(), TimeSpan.FromHours(1));

            // Build reset URL - No encoding needed, token is already safe (GUID)
            var frontendBase = GetFrontendBaseUrl();
            var resetUrl = $"{frontendBase.TrimEnd('/')}/reset-password?token={resetToken}";
            
            // Send email
            var subject = "Emxinh.shop: Đặt lại mật khẩu";
            var html = LoadResetPasswordTemplate(resetUrl, 1);

            try
            {
                await _emailService.SendEmailAsync(user.Email, subject, html);
            }
            catch (Exception)
            {
                await _redisDb.KeyDeleteAsync(key);
                // Don't reveal the error, just return generic success
                return new AppApiResponse { Success = true, Message = "Nếu email tồn tại, link reset mật khẩu đã được gửi" };
            }

            return new AppApiResponse { Success = true, Message = "Nếu email tồn tại, link reset mật khẩu đã được gửi" };
        }

        public async Task<AppApiResponse> ResetPassword(ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
                return new AppApiResponse { Success = false, Message = "Dữ liệu không hợp lệ" };

            // Check token in Redis
            var key = $"reset:password:{request.Token}";
            var val = await _redisDb.StringGetAsync(key);
            
            if (val.IsNullOrEmpty)
                return new AppApiResponse { Success = false, Message = "Token không hợp lệ hoặc đã hết hạn" };

            if (!Guid.TryParse(val.ToString(), out var userId))
                return new AppApiResponse { Success = false, Message = "Token không hợp lệ" };

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new AppApiResponse { Success = false, Message = "Người dùng không tồn tại" };

            // Update password
            user.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.SaveAsync();

            // Delete the token so it can't be reused
            await _redisDb.KeyDeleteAsync(key);

            // Invalidate all refresh tokens for this user (force re-login on all devices)
            await _redisDb.KeyDeleteAsync($"refresh:{user.Id}");

            return new AppApiResponse { Success = true, Message = "Đặt lại mật khẩu thành công" };
        }

        private string LoadResetPasswordTemplate(string resetUrl, int expiryHours)
        {
            try
            {
                var baseDir = System.AppContext.BaseDirectory ?? ".";
                var path = System.IO.Path.Combine(baseDir, "EmailTemplates", "reset_password_template.html");
                string template = System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path) : null!;
                
                if (string.IsNullOrWhiteSpace(template))
                {
                    // Fallback simple HTML
                    template = $"<p>Bạn đã yêu cầu đặt lại mật khẩu. Vui lòng click vào link sau: <a href=\"{System.Net.WebUtility.HtmlEncode(resetUrl)}\">Đặt lại mật khẩu</a></p>" +
                               $"<p>Link này sẽ hết hạn sau {expiryHours} giờ.</p>";
                }

                template = template.Replace("{{resetUrl}}", resetUrl);
                template = template.Replace("{{siteName}}", "Emxinh.shop");
                template = template.Replace("{{expiryHours}}", expiryHours.ToString());
                template = template.Replace("{{buttonText}}", "Đặt lại mật khẩu");
                template = template.Replace("{{year}}", DateTime.UtcNow.Year.ToString());
                return template;
            }
            catch
            {
                return $"<p>Bạn đã yêu cầu đặt lại mật khẩu. Vui lòng click vào link sau: <a href=\"{System.Net.WebUtility.HtmlEncode(resetUrl)}\">Đặt lại mật khẩu</a></p>" +
                       $"<p>Link này sẽ hết hạn sau {expiryHours} giờ.</p>";
            }
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
