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
        private readonly IOrderRepository _orderRepo;

        public CouponService(ICouponRepository couponRepo, IOrderRepository orderRepo)
        {
            _couponRepo = couponRepo;
            _orderRepo = orderRepo;
        }

        public async Task<List<CouponResponse>> GetAllAsync()
        {
            var coupons = await _couponRepo.GetAllAsync();
            if (!coupons.Any())
                return new List<CouponResponse>();

            // Bulk load reserved counts to avoid N+1 query
            var couponIds = coupons.Select(c => c.CouponId).ToList();
            var reservedCounts = await _orderRepo.CountOrderCouponsByCouponIdsAsync(couponIds, "Reserved");

            return coupons.Select(c => new CouponResponse
            {
                CouponId = c.CouponId,
                Code = c.Code,
                DiscountPercent = c.DiscountPercent,
                MinOrderAmount = c.MinOrderAmount,
                MaxDiscountAmount = c.MaxDiscountAmount,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                UsageLimit = c.UsageLimit,
                UsedCount = c.UsedCount,
                ReservedCount = reservedCounts.TryGetValue(c.CouponId, out var count) ? count : 0,
                IsActive = c.IsActive
            }).ToList();
        }

        public async Task<bool> CanReserveCouponAsync(Guid couponId)
        {
            var coupon = await _couponRepo.GetByIdAsync(couponId);
            if (coupon == null) return false;

            if (!coupon.UsageLimit.HasValue) return true;

            // Count reserved and applied coupons
            var count = await _orderRepo.CountOrderCouponsByCouponIdAsync(couponId, "Reserved", "Applied");
            return count < coupon.UsageLimit.Value;
        }

        public async Task<bool> ConfirmCouponUsageForOrderAsync(Guid orderId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null) return false;

            bool anyChanged = false;

            foreach (var oc in order.OrderCoupons.Where(oc => oc.Status == "Reserved").ToList())
            {
                var coupon = await _couponRepo.GetByIdAsync(oc.CouponId);
                if (coupon == null)
                {
                    // release reservation
                    oc.Status = "Released";
                    anyChanged = true;
                    continue;
                }

                // Check usage limit atomically by re-fetching coupon
                if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
                {
                    // cannot apply, release
                    oc.Status = "Released";
                    anyChanged = true;
                    continue;
                }

                // Apply coupon: increment UsedCount and mark order coupon applied
                coupon.UsedCount += 1;
                oc.Status = "Applied";
                oc.AppliedAt = DateTime.UtcNow;

                // If usage limit reached, deactivate coupon to make it clearly unusable
                if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
                {
                    coupon.IsActive = false;
                }

                await _couponRepo.UpdateAsync(coupon);
                anyChanged = true;
            }

            if (anyChanged)
            {
                await _orderRepo.UpdateAsync(order);
                await _couponRepo.SaveAsync();
                await _orderRepo.SaveAsync();
            }

            return true;
        }

        public async Task<bool> ReleaseCouponReservationsForOrderAsync(Guid orderId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null) return false;

            bool anyChanged = false;
            foreach (var oc in order.OrderCoupons.Where(oc => oc.Status == "Reserved").ToList())
            {
                oc.Status = "Released";
                anyChanged = true;
            }

            if (anyChanged)
            {
                await _orderRepo.UpdateAsync(order);
                await _orderRepo.SaveAsync();
            }

            return true;
        }
        public async Task<CouponResponse?> GetByIdAsync(Guid id)
        {
            var coupon = await _couponRepo.GetByIdAsync(id);
            return coupon.IsNull() ? null : await MapToResponseAsync(coupon!);
        }

        public async Task<CouponResponse?> GetByCodeAsync(string code)
        {
            var coupon = await _couponRepo.GetByCodeAsync(code);
            return coupon.IsNull() ? null : await MapToResponseAsync(coupon!);
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
                UserId = request.UserId,
                CouponType = request.CouponType,
                DiscountPercent = request.DiscountPercent,
                MinOrderAmount = request.MinOrderAmount,
                MaxDiscountAmount = request.MaxDiscountAmount,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                UsageLimit = request.UsageLimit,
                IsActive = request.IsActive
            };

            await _couponRepo.CreateAsync(coupon);
            return await MapToResponseAsync(coupon);
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
            return await MapToResponseAsync(coupon);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _couponRepo.DeleteAsync(id);
        }

        public async Task<List<CouponResponse>> GetActiveCouponsAsync()
        {
            var coupons = await _couponRepo.GetActiveCouponsAsync();
            if (!coupons.Any())
                return new List<CouponResponse>();

            // Bulk load reserved counts to avoid N+1 query
            var couponIds = coupons.Select(c => c.CouponId).ToList();
            var reservedCounts = await _orderRepo.CountOrderCouponsByCouponIdsAsync(couponIds, "Reserved");

            return coupons.Select(c => new CouponResponse
            {
                CouponId = c.CouponId,
                Code = c.Code,
                DiscountPercent = c.DiscountPercent,
                MinOrderAmount = c.MinOrderAmount,
                MaxDiscountAmount = c.MaxDiscountAmount,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                UsageLimit = c.UsageLimit,
                UsedCount = c.UsedCount,
                ReservedCount = reservedCounts.TryGetValue(c.CouponId, out var count) ? count : 0,
                IsActive = c.IsActive
            }).ToList();
        }

        public async Task<List<CouponResponse>> GetUserCouponsAsync(Guid userId)
        {
            var coupons = await _couponRepo.GetByUserIdAsync(userId);
            if (!coupons.Any()) return new List<CouponResponse>();

            var responses = new List<CouponResponse>();
            foreach (var c in coupons)
            {
                responses.Add(await MapToResponseAsync(c));
            }
            return responses;
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

        public async Task<bool> HasUserWelcomeCouponAsync(Guid userId)
        {
            return await _couponRepo.ExistsByPredicateAsync(c => c.UserId == userId && c.CouponType == "Welcome");
        }

        public async Task<WelcomeCouponSettingsResponse?> GetWelcomeCouponSettingsAsync()
        {
            var settings = await _couponRepo.GetWelcomeCouponSettingsAsync();
            if (settings == null)
            {
                // Create default settings if not exists
                settings = new WelcomeCouponSettings
                {
                    Id = Guid.NewGuid(),
                    DiscountPercent = 10,
                    MinOrderAmount = 100000,
                    MaxDiscountAmount = 50000,
                    UsageLimit = 1,
                    ValidityMonths = 1,
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow
                };
                settings = await _couponRepo.UpdateWelcomeCouponSettingsAsync(settings);
            }

            return new WelcomeCouponSettingsResponse
            {
                Id = settings.Id,
                DiscountPercent = settings.DiscountPercent,
                MinOrderAmount = settings.MinOrderAmount,
                MaxDiscountAmount = settings.MaxDiscountAmount,
                UsageLimit = settings.UsageLimit,
                ValidityMonths = settings.ValidityMonths,
                IsEnabled = settings.IsEnabled,
                CreatedAt = settings.CreatedAt,
                UpdatedAt = settings.UpdatedAt
            };
        }

        public async Task<WelcomeCouponSettingsResponse> UpdateWelcomeCouponSettingsAsync(UpdateWelcomeCouponSettingsRequest request)
        {
            var settings = new WelcomeCouponSettings
            {
                DiscountPercent = request.DiscountPercent,
                MinOrderAmount = request.MinOrderAmount,
                MaxDiscountAmount = request.MaxDiscountAmount,
                UsageLimit = request.UsageLimit,
                ValidityMonths = request.ValidityMonths,
                IsEnabled = request.IsEnabled
            };

            var updated = await _couponRepo.UpdateWelcomeCouponSettingsAsync(settings);

            return new WelcomeCouponSettingsResponse
            {
                Id = updated.Id,
                DiscountPercent = updated.DiscountPercent,
                MinOrderAmount = updated.MinOrderAmount,
                MaxDiscountAmount = updated.MaxDiscountAmount,
                UsageLimit = updated.UsageLimit,
                ValidityMonths = updated.ValidityMonths,
                IsEnabled = updated.IsEnabled,
                CreatedAt = updated.CreatedAt,
                UpdatedAt = updated.UpdatedAt
            };
        }

        private async Task<CouponResponse> MapToResponseAsync(Coupon coupon)
        {
            // Count current reservations for this coupon (status = Reserved)
            var reservedCount = await _orderRepo.CountOrderCouponsByCouponIdAsync(coupon.CouponId, "Reserved");

            return new CouponResponse
            {
                CouponId = coupon.CouponId,
                Code = coupon.Code,
                UserId = coupon.UserId,
                CouponType = coupon.CouponType,
                DiscountPercent = coupon.DiscountPercent,
                MinOrderAmount = coupon.MinOrderAmount,
                MaxDiscountAmount = coupon.MaxDiscountAmount,
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate,
                UsageLimit = coupon.UsageLimit,
                UsedCount = coupon.UsedCount,
                ReservedCount = reservedCount,
                IsActive = coupon.IsActive
            };
        }
    }
}