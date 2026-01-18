using NextShopV2.Domain.Entities.Marketing;

namespace NextShopV2.Application.Interfaces
{
    public interface IAnnouncementRepository
    {
        Task<Announcement?> GetActiveAsync();
        Task<List<Announcement>> GetAllAsync();
        Task<Announcement?> GetByIdAsync(Guid id);
        Task AddAsync(Announcement announcement);
        Task UpdateAsync(Announcement announcement);
        Task DeleteAsync(Announcement announcement);
        Task SaveAsync();
    }
}