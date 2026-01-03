using NextShopV2.Domain.Entities.Marketing;
namespace NextShopV2.Application.Interfaces
{
    public interface IAdvertisementRepository
    {
        Task<List<Advertisement>> GetAllAsync();
        Task<Advertisement?> GetByIdAsync(Guid id);
        Task<List<Advertisement>> GetByIdsAsync(List<Guid> ids);
        Task AddAsync(Advertisement advertisement);
        Task UpdateAsync(Advertisement advertisement);
        Task DeleteAsync(Advertisement advertisement);
        Task DeleteRangeAsync(List<Advertisement> advertisements);
        Task SaveAsync();
    }
}
