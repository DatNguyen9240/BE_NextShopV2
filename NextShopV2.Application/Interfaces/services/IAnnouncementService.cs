using NextShopV2.Domain.Entities.Marketing;

namespace NextShopV2.Application.Interfaces
{
    public interface IAnnouncementService
    {
        Task<Announcement?> GetActiveAsync();
        Task<List<Announcement>> GetAllAsync();
        Task<Announcement> CreateAsync(Announcement announcement);
        Task<bool> UpdateAsync(Guid id, Announcement announcement);
        Task<bool> DeleteAsync(Guid id);
    }
}