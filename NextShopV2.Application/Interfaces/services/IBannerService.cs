using NextShopV2.Domain.Entities.Marketing;
using NextShopV2.Application.DTOs.Request.CreateDto;
namespace NextShopV2.Application.Interfaces
{
    public interface IBannerService
    {
    Task<List<Advertisement>> GetAllAsync(string? type = null);
    Task<Advertisement?> GetByIdAsync(Guid id);
    Task<Advertisement> CreateAsync(BannerRequestDto dto);
    Task<bool> UpdateAsync(Guid id, BannerRequestDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<Advertisement?> PatchAsync(Guid id, BannerRequestDto dto);
    }
}
