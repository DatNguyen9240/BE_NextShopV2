using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;

namespace NextShopV2.Application.Interfaces.Services
{
    public interface ICouponService
    {
        Task<List<CouponResponse>> GetAllAsync();
        Task<CouponResponse?> GetByIdAsync(Guid id);
        Task<CouponResponse?> GetByCodeAsync(string code);
        Task<CouponResponse> CreateAsync(CreateCouponRequest request);
        Task<CouponResponse?> UpdateAsync(Guid id, UpdateCouponRequest request);
        Task<bool> DeleteAsync(Guid id);
        Task<List<CouponResponse>> GetActiveCouponsAsync();
        Task<bool> ValidateCouponAsync(string code);
        Task<decimal> CalculateDiscountAsync(string couponCode, decimal originalAmount);
        Task<bool> CanApplyCouponAsync(string couponCode, decimal orderAmount);

        // Reservation flow
        Task<bool> CanReserveCouponAsync(Guid couponId);
        Task<bool> ConfirmCouponUsageForOrderAsync(Guid orderId);
        Task<bool> ReleaseCouponReservationsForOrderAsync(Guid orderId);

        // Welcome coupon management
        Task<bool> HasUserWelcomeCouponAsync(Guid userId);
        Task<WelcomeCouponSettingsResponse?> GetWelcomeCouponSettingsAsync();
        Task<WelcomeCouponSettingsResponse> UpdateWelcomeCouponSettingsAsync(UpdateWelcomeCouponSettingsRequest request);
    }
}