using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using NextShopV2.Domain.Entities.Marketing;
using NextShopV2.Shared.Interfaces;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.Interfaces;

namespace NextShopV2.Application.Services
{
    /// <summary>
    /// Service for managing advertisement/banner cache operations
    /// </summary>
    public class AdvertisementCacheService : IAdvertisementCacheService
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly ICacheService _cacheService;
        private readonly TimeSpan _expiry = TimeSpan.FromMinutes(5);

        public AdvertisementCacheService(IBannerRepository bannerRepository, ICacheService cacheService)
        {
            _bannerRepository = bannerRepository;
            _cacheService = cacheService;
        }

        /// <summary>
        /// Update banners cache from database
        /// </summary>
        public async Task UpdateBannerCacheAsync()
        {
            var cacheKey = "banners:all";
            var banners = await _bannerRepository.GetAllAsync();
            var sortedBanners = banners.OrderBy(a => a.SortOrder).ToList();
            
            await _cacheService.SetAsync(cacheKey, sortedBanners, _expiry);
        }

        /// <summary>
        /// Get banners from cache or database
        /// </summary>
        public async Task<List<Advertisement>> GetBannersAsync()
        {
            var cacheKey = "banners:all";
            var cachedBanners = await _cacheService.GetAsync<List<Advertisement>>(cacheKey);
            
            if (cachedBanners != null)
                return cachedBanners;

            // If not in cache, get from database and cache it
            var banners = await _bannerRepository.GetAllAsync();
            var sortedBanners = banners.OrderBy(a => a.SortOrder).ToList();
            
            await _cacheService.SetAsync(cacheKey, sortedBanners, _expiry);
            return sortedBanners;
        }

        /// <summary>
        /// Invalidate banners cache
        /// </summary>
        public async Task InvalidateBannerCacheAsync()
        {
            var cacheKey = "banners:all";
            await _cacheService.RemoveAsync(cacheKey);
        }


    }
}