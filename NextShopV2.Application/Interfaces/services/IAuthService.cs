using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
using System;
using AppApiResponse = NextShopV2.Application.DTOs.Response.ApiResponse;
using AppAuthResponse = NextShopV2.Application.DTOs.Response.AuthResponse;

namespace NextShopV2.Application.Interfaces
{
    public interface IAuthService
    {
        AppApiResponse Register(RegisterRequest request);
        AppAuthResponse Login(LoginRequest request);
        AppAuthResponse Refresh(RefreshTokenRequest request);
        AppApiResponse Logout(string accessToken, string refreshToken);
        UserResponse? GetMe(Guid userId);

        // Profile & address management
        AppApiResponse UpdateProfile(Guid userId, UpdateProfileRequest request);
        AddressResponse? UpsertAddress(Guid userId, UpdateAddressRequest request);
        bool DeleteAddress(Guid userId, Guid addressId);
    }
}