using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
using System;
using AppApiResponse = NextShopV2.Application.DTOs.Response.ApiResponse;
using AppAuthResponse = NextShopV2.Application.DTOs.Response.AuthResponse;

namespace NextShopV2.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AppApiResponse> Register(RegisterRequest request);
        Task<AppAuthResponse> Login(LoginRequest request);
        Task<AppAuthResponse> Refresh(RefreshTokenRequest request);
        Task<AppApiResponse> Logout(string accessToken, string refreshToken);
        Task<UserResponse?> GetMe(Guid userId);

        // Profile & address management
        Task<AppApiResponse> UpdateProfile(Guid userId, UpdateProfileRequest request);
        Task<AddressResponse?> UpsertAddress(Guid userId, UpdateAddressRequest request);
        Task<bool> DeleteAddress(Guid userId, Guid addressId);

        // Email OTP (MFA) support
        Task<AppApiResponse> StartEmailOtp(NextShopV2.Application.DTOs.Request.StartEmailOtpRequest request);
        Task<AppAuthResponse> VerifyEmailOtp(NextShopV2.Application.DTOs.Request.VerifyEmailOtpRequest request);

        // Enroll/Unenroll MFA for authenticated users
        Task<AppApiResponse> StartEnableEmailMfa(System.Guid userId);
        Task<AppApiResponse> VerifyEnableEmailMfa(NextShopV2.Application.DTOs.Request.VerifyEmailOtpRequest request, System.Guid userId);
        Task<AppApiResponse> DisableEmailMfa(System.Guid userId);

        // Email verification for registration / Google sign-in
        Task<AppApiResponse> StartEmailVerification(System.Guid userId, string email);
        Task<AppAuthResponse> VerifyEmailToken(string token);

        // Google Sign-In (signin only)
        Task<AppAuthResponse> GoogleSignIn(string idToken);
        // Google Sign-Up (create account from Google and send verification)
        Task<AppAuthResponse> GoogleRegister(string idToken);

        // Forgot Password & Reset Password
        Task<AppApiResponse> ForgotPassword(ForgotPasswordRequest request);
        Task<AppApiResponse> ResetPassword(ResetPasswordRequest request);
    }
}