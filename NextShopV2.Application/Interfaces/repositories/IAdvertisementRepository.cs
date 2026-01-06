using NextShopV2.Domain.Entities.Marketing;
namespace NextShopV2.Application.Interfaces
{
    public interface IAdvertisementRepository
    {
        Task<List<Advertisement>> GetAllAsync();
        Task<List<Advertisement>> GetByTypeAsync(string type);
        Task<Advertisement?> GetByIdAsync(Guid id);
        Task<List<Advertisement>> GetByIdsAsync(List<Guid> ids);
        Task AddAsync(Advertisement advertisement);
        Task UpdateAsync(Advertisement advertisement);
        Task DeleteAsync(Advertisement advertisement);
        Task DeleteRangeAsync(List<Advertisement> advertisements);

        // Shift sort orders within a type. startInclusive..endInclusive will be shifted by delta (+1 or -1)
        Task ShiftSortOrdersInRangeAsync(string type, int startInclusive, int endInclusive, int delta);
        // Convenience helpers
        Task IncrementSortOrdersFromAsync(string type, int fromOrder);
        Task DecrementSortOrdersAfterAsync(string type, int afterOrder);

        Task SaveAsync();
    }
}
