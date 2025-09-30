using NextShopV2.Domain.Entities.Marketing;
namespace NextShopV2.Application.Interfaces
{
    public interface IBannerRepository
    {
        Task<List<Advertisement>> GetAllAsync();
        Task<Advertisement?> GetByIdAsync(Guid id);
        Task AddAsync(Advertisement banner);
        Task UpdateAsync(Advertisement banner);
        Task DeleteAsync(Advertisement banner);
        Task SaveAsync();
    }
}
