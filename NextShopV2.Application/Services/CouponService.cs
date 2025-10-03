using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Domain.Entities.Coupons;
using NextShopV2.Shared.Extensions;

namespace NextShopV2.Application.Services
{
    public class CouponService : ICouponService
    {
        private readonly ICouponRepository _couponRepo;

        public CouponService(ICouponRepository couponRepo)
        {
            _couponRepo = couponRepo;
        }

        public async Task<List<CouponResponse>> GetAllAsync()
        {
            var coupons = await _couponRepo.GetAllAsync();
            return coupons.Select(MapToResponse).ToList();
        }

        public async Task<CouponResponse?> GetByIdAsync(Guid id)
        {
            var coupon = await _couponRepo.GetByIdAsync(id);
            return coupon.IsNull() ? null : MapToResponse(coupon!);
        }

        public async Task<CouponResponse?> GetByCodeAsync(string code)
        {
            var coupon = await _couponRepo.GetByCodeAsync(code);
            return coupon.IsNull() ? null : MapToResponse(coupon!);
        }

        public async Task<CouponResponse> CreateAsync(CreateCouponRequest request)
        {
            // Check if coupon code already exists
            if (await _couponRepo.ExistsAsync(request.Code))
                throw new ArgumentException($"Coupon code '{request.Code}' already exists");

            // Validate dates
            if (request.EndDate <= request.StartDate)
                throw new ArgumentException("End date must be after start date");

            var coupon = new Coupon
            {
                CouponId = Guid.NewGuid(),
                Code = request.Code.ToUpper().Trim(),
                DiscountPercent = request.DiscountPercent,
                MinOrderAmount = request.MinOrderAmount,
                MaxDiscountAmount = request.MaxDiscountAmount,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                UsageLimit = request.UsageLimit,
                IsActive = request.IsActive
            };

            await _couponRepo.CreateAsync(coupon);
            return MapToResponse(coupon);
        }

        public async Task<CouponResponse?> UpdateAsync(Guid id, UpdateCouponRequest request)
        {
            var coupon = await _couponRepo.GetByIdAsync(id);
            if (coupon.IsNull())
                return null;

            // Check if new code already exists (excluding current coupon)
            if (!string.IsNullOrEmpty(request.Code))
            {
                var existingCoupon = await _couponRepo.GetByCodeAsync(request.Code);
                if (existingCoupon != null && existingCoupon.CouponId != id)
                    throw new ArgumentException($"Coupon code '{request.Code}' already exists");
                
                coupon!.Code = request.Code.ToUpper().Trim();
            }


            if (request.DiscountPercent.HasValue)
                coupon!.DiscountPercent = request.DiscountPercent.Value;

            if (request.MinOrderAmount.HasValue)
                coupon!.MinOrderAmount = request.MinOrderAmount.Value;

            if (request.MaxDiscountAmount.HasValue)
                coupon!.MaxDiscountAmount = request.MaxDiscountAmount.Value;


            if (request.StartDate.HasValue)
                coupon!.StartDate = request.StartDate.Value;

            if (request.EndDate.HasValue)
                coupon!.EndDate = request.EndDate.Value;

            if (request.UsageLimit.HasValue)
                coupon!.UsageLimit = request.UsageLimit.Value;

            if (request.IsActive.HasValue)
                coupon!.IsActive = request.IsActive.Value;

            // Validate dates after update
            if (coupon!.EndDate <= coupon.StartDate)
                throw new ArgumentException("End date must be after start date");

            await _couponRepo.UpdateAsync(coupon);
            return MapToResponse(coupon);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _couponRepo.DeleteAsync(id);
        }

        public async Task<List<CouponResponse>> GetActiveCouponsAsync()
        {
            var coupons = await _couponRepo.GetActiveCouponsAsync();
            return coupons.Select(MapToResponse).ToList();
        }

        public async Task<bool> ValidateCouponAsync(string code)
        {
            var coupon = await _couponRepo.GetByCodeAsync(code);
            if (coupon.IsNull())
                return false;

            var now = DateTime.UtcNow;
            return coupon!.IsActive && 
                   coupon.StartDate <= now && 
                   coupon.EndDate >= now;
        }

        public async Task<decimal> CalculateDiscountAsync(string couponCode, decimal originalAmount)
        {
            var coupon = await _couponRepo.GetByCodeAsync(couponCode);
            if (coupon.IsNull())
                return 0;

            // Validate coupon
            if (!IsValidCoupon(coupon!, originalAmount))
                return 0;

            // Calculate discount
            var discountAmount = originalAmount * (coupon!.DiscountPercent / 100);

            // Apply max discount limit if set
            if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value)
                discountAmount = coupon.MaxDiscountAmount.Value;

            return discountAmount;
        }

        public async Task<bool> CanApplyCouponAsync(string couponCode, decimal orderAmount)
        {
            var coupon = await _couponRepo.GetByCodeAsync(couponCode);
            if (coupon.IsNull())
                return false;

            return IsValidCoupon(coupon!, orderAmount);
        }

        private bool IsValidCoupon(Coupon coupon, decimal orderAmount)
        {
            var now = DateTime.UtcNow;

            // Check basic validity
            if (!coupon.IsActive || coupon.StartDate > now || coupon.EndDate < now)
                return false;

            // Check minimum order amount
            if (coupon.MinOrderAmount.HasValue && orderAmount < coupon.MinOrderAmount.Value)
                return false;

            // Check usage limit
            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
                return false;

            return true;
        }

        private CouponResponse MapToResponse(Coupon coupon)
        {
            return new CouponResponse
            {
                CouponId = coupon.CouponId,
                Code = coupon.Code,
                DiscountPercent = coupon.DiscountPercent,
                MinOrderAmount = coupon.MinOrderAmount,
                MaxDiscountAmount = coupon.MaxDiscountAmount,
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate,
                UsageLimit = coupon.UsageLimit,
                UsedCount = coupon.UsedCount,
                IsActive = coupon.IsActive
            };
        }
    }
}