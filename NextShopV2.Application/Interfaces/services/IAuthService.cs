using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
namespace NextShopV2.Application.Interfaces
{
    public interface IAuthService
    {
    ApiResponse Register(RegisterRequest request);
        AuthResponse Login(LoginRequest request);
        AuthResponse Refresh(RefreshTokenRequest request);
    }
}