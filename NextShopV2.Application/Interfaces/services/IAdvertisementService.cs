using NextShopV2.Domain.Entities.Marketing;
using NextShopV2.Application.DTOs.Request.CreateDto;
namespace NextShopV2.Application.Interfaces
{
    public interface IAdvertisementService
    {
    Task<List<Advertisement>> GetAllAsync(string? type = null);
    Task<Advertisement?> GetByIdAsync(Guid id);
    Task<Advertisement> CreateAsync(AdvertisementRequestDto dto);
    Task<bool> UpdateAsync(Guid id, AdvertisementRequestDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<Advertisement?> PatchAsync(Guid id, AdvertisementRequestDto dto);
    }
}
