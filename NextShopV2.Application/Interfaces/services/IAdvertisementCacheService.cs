using NextShopV2.Domain.Entities.Marketing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Application.Interfaces.Services
{
    /// <summary>
    /// Interface for advertisement cache operations
    /// </summary>
    public interface IAdvertisementCacheService
    {
        Task UpdateBannerCacheAsync();
        Task<List<Advertisement>> GetBannersAsync();
        Task InvalidateBannerCacheAsync();
    }
}