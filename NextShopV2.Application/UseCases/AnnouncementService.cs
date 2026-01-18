using NextShopV2.Application.Interfaces;
using NextShopV2.Domain.Entities.Marketing;

namespace NextShopV2.Application.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _repo;

        public AnnouncementService(IAnnouncementRepository repo)
        {
            _repo = repo;
        }

        public async Task<Announcement?> GetActiveAsync()
        {
            return await _repo.GetActiveAsync();
        }

        public async Task<List<Announcement>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Announcement> CreateAsync(Announcement announcement)
        {
            announcement.UpdatedAt = DateTime.UtcNow;
            await _repo.AddAsync(announcement);
            await _repo.SaveAsync();
            return announcement;
        }

        public async Task<bool> UpdateAsync(Guid id, Announcement announcement)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            existing.ClassName = announcement.ClassName;
            existing.IsActive = announcement.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            if (announcement.IsActive)
            {
                // Deactivate all others
                var all = await _repo.GetAllAsync();
                foreach (var a in all.Where(a => a.Id != id && a.IsActive))
                {
                    a.IsActive = false;
                    await _repo.UpdateAsync(a);
                }
            }

            await _repo.UpdateAsync(existing);
            await _repo.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            await _repo.DeleteAsync(existing);
            await _repo.SaveAsync();
            return true;
        }
    }
}