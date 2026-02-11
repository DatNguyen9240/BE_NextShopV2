using Microsoft.EntityFrameworkCore;
using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Domain.Entities.Coupons;
using NextShopV2.Infrastructure.Persistence;

namespace NextShopV2.Infrastructure.Repositories
{
    public class CouponRepository : ICouponRepository
    {
        private readonly AppDbContext _context;

        public CouponRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Coupon>> GetAllAsync()
        {
            return await _context.Coupons
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();
        }

        public async Task<Coupon?> GetByIdAsync(Guid id)
        {
            return await _context.Coupons
                .FirstOrDefaultAsync(c => c.CouponId == id);
        }

        public async Task<Coupon?> GetByCodeAsync(string code)
        {
            return await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper());
        }

        public async Task<IEnumerable<Coupon>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Coupons
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();
        }

        public async Task<Coupon> CreateAsync(Coupon coupon)
        {
            _context.Coupons.Add(coupon);
            await SaveAsync();
            return coupon;
        }

        public async Task<Coupon> UpdateAsync(Coupon coupon)
        {
            _context.Coupons.Update(coupon);
            await SaveAsync();
            return coupon;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var coupon = await GetByIdAsync(id);
            if (coupon == null)
                return false;

            _context.Coupons.Remove(coupon);
            await SaveAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string code)
        {
            return await _context.Coupons
                .AnyAsync(c => c.Code.ToUpper() == code.ToUpper());
        }

        public async Task<IEnumerable<Coupon>> GetActiveCouponsAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Coupons
                .Where(c => c.IsActive && c.StartDate <= now && c.EndDate >= now)
                .OrderByDescending(c => c.DiscountPercent)
                .ToListAsync();
        }

        public async Task<bool> ExistsByPredicateAsync(System.Linq.Expressions.Expression<Func<Coupon, bool>> predicate)
        {
            return await _context.Coupons.AnyAsync(predicate);
        }

        public async Task<WelcomeCouponSettings?> GetWelcomeCouponSettingsAsync()
        {
            return await _context.WelcomeCouponSettings.FirstOrDefaultAsync();
        }

        public async Task<WelcomeCouponSettings> UpdateWelcomeCouponSettingsAsync(WelcomeCouponSettings settings)
        {
            var existing = await GetWelcomeCouponSettingsAsync();
            if (existing != null)
            {
                existing.DiscountPercent = settings.DiscountPercent;
                existing.MinOrderAmount = settings.MinOrderAmount;
                existing.MaxDiscountAmount = settings.MaxDiscountAmount;
                existing.UsageLimit = settings.UsageLimit;
                existing.ValidityMonths = settings.ValidityMonths;
                existing.IsEnabled = settings.IsEnabled;
                existing.UpdatedAt = DateTime.UtcNow;
                _context.WelcomeCouponSettings.Update(existing);
            }
            else
            {
                _context.WelcomeCouponSettings.Add(settings);
            }
            await SaveAsync();
            return existing ?? settings;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}