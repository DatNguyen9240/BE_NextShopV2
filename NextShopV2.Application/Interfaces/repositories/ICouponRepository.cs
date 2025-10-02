using NextShopV2.Domain.Entities.Coupons;

namespace NextShopV2.Application.Interfaces.Repositories
{
    public interface ICouponRepository
    {
        Task<IEnumerable<Coupon>> GetAllAsync();
        Task<Coupon?> GetByIdAsync(Guid id);
        Task<Coupon?> GetByCodeAsync(string code);
        Task<Coupon> CreateAsync(Coupon coupon);
        Task<Coupon> UpdateAsync(Coupon coupon);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(string code);
        Task<IEnumerable<Coupon>> GetActiveCouponsAsync();
        Task SaveAsync();
    }
}