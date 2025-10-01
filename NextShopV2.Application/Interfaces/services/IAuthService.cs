using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
using System;

namespace NextShopV2.Application.Interfaces
{
    public interface IAuthService
    {
        ApiResponse Register(RegisterRequest request);
        AuthResponse Login(LoginRequest request);
        AuthResponse Refresh(RefreshTokenRequest request);
        UserResponse? GetMe(Guid userId);
    }
}