using NextShopV2.Application.Interfaces;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Shared.Interfaces;
using NextShopV2.Domain.Entities.Marketing;
using NextShopV2.Application.DTOs.Request.CreateDto;
using NextShopV2.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace NextShopV2.Application.Services
{
    public class BannerService : IBannerService
    {
        private readonly IBannerRepository _repo;
        private readonly IOrderResolutionService _orderResolutionService;
        private readonly ILoggingService _loggingService;
        
        public BannerService(IBannerRepository repo, IOrderResolutionService orderResolutionService, ILoggingService loggingService)
        {
            _repo = repo;
            _orderResolutionService = orderResolutionService;
            _loggingService = loggingService;
        }
        public async Task<List<Advertisement>> GetAllAsync(string? type = null)
        {
            var banners = await _repo.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(type))
            {
                banners = banners.Where(b => string.Equals(b.Type, type, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Sort by SortOrder first, then by Id for consistency when SortOrder is same
            return banners.OrderBy(b => b.SortOrder).ThenBy(b => b.Id).ToList();
        }
        public async Task<Advertisement?> GetByIdAsync(Guid id)
            => await _repo.GetByIdAsync(id);
    public async Task<Advertisement> CreateAsync(BannerRequestDto dto)
        {
            // Auto-resolve SortOrder conflict using OrderResolutionService
            var existingBanners = await _repo.GetAllAsync();
            var existingSortOrders = existingBanners.Select(b => b.SortOrder);
            var resolvedSortOrder = _orderResolutionService.ResolveOrder(existingSortOrders, dto.SortOrder ?? 0);

            var banner = new Advertisement
            {
                Id = Guid.NewGuid(),
                PublicId = dto.PublicId ?? "",
                Title = dto.Title ?? string.Empty,
                ImageUrl = dto.ImageUrl ?? string.Empty,
                Type = dto.Type ?? string.Empty,
                SortOrder = resolvedSortOrder, // ← Use resolved SortOrder
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(banner);
            await _repo.SaveAsync();
            return banner;
        }
    public async Task<bool> UpdateAsync(Guid id, BannerRequestDto dto)
        {
            var exist = await _repo.GetByIdAsync(id);
            if (exist.IsNull()) return false;

            // Auto-resolve SortOrder conflict for update using OrderResolutionService
            var resolvedSortOrder = exist!.SortOrder;
            if (dto.SortOrder.HasValue)
            {
                var existingBanners = await _repo.GetAllAsync();
                var existingSortOrders = existingBanners.Select(b => b.SortOrder);
                resolvedSortOrder = _orderResolutionService.ResolveOrder(existingSortOrders, dto.SortOrder.Value, exist.SortOrder);
            }

            // Update only allowed fields
            if (dto.PublicId != null) exist.PublicId = dto.PublicId;
            if (dto.Title != null) exist.Title = dto.Title;
            if (dto.ImageUrl != null) exist.ImageUrl = dto.ImageUrl;
            if (dto.Type != null) exist.Type = dto.Type;
            if (dto.SortOrder.HasValue) exist.SortOrder = resolvedSortOrder; // ← Use resolved SortOrder
            // Do NOT update CreatedAt
            await _repo.UpdateAsync(exist);
            await _repo.SaveAsync();
            return true;
        }
        public async Task<bool> DeleteAsync(Guid id)
        {
            var banner = await _repo.GetByIdAsync(id);
            if (banner.IsNull()) 
            {
                _loggingService.LogEntityNotFound("Banner", id);
                return false;
            }
            
            // Delete from database
            await _repo.DeleteAsync(banner!);
            await _repo.SaveAsync();
            
            _loggingService.LogEntityDeleted("Banner", id);
            return true;
        }
        public async Task<Advertisement?> PatchAsync(Guid id, BannerRequestDto dto)
        {
            var banner = await _repo.GetByIdAsync(id);
            if (banner.IsNull()) return null;

            // Auto-resolve SortOrder conflict for patch using OrderResolutionService
            var resolvedSortOrder = banner!.SortOrder;
            if (dto.SortOrder.HasValue)
            {
                var existingBanners = await _repo.GetAllAsync();
                var existingSortOrders = existingBanners.Select(b => b.SortOrder);
                resolvedSortOrder = _orderResolutionService.ResolveOrder(existingSortOrders, dto.SortOrder.Value, banner.SortOrder);
            }

            if (dto.PublicId != null) banner.PublicId = dto.PublicId;
            if (dto.Title != null) banner.Title = dto.Title;
            if (dto.ImageUrl != null) banner.ImageUrl = dto.ImageUrl;
            if (dto.Type != null) banner.Type = dto.Type;
            if (dto.SortOrder.HasValue) banner.SortOrder = resolvedSortOrder; // ← Use resolved SortOrder
            await _repo.UpdateAsync(banner);
            await _repo.SaveAsync();
            return banner;
        }

        /// <summary>
        /// Delete multiple banners - Example usage of SafeForEachAsync
        /// </summary>
        public async Task<bool> DeleteMultipleBannersAsync(List<Guid> bannerIds)
        {
            if (bannerIds.IsNullOrEmpty())
            {
                _loggingService.LogValidationFailure("BulkBannerDeletion", "Empty or null banner IDs");
                return false;
            }

            var banners = await _repo.GetByIdsAsync(bannerIds);
            
            if (banners.IsNullOrEmpty())
            {
                _loggingService.LogValidationFailure("BulkBannerDeletion", new { BannerIds = bannerIds, Reason = "No banners found" });
                return false;
            }

            _loggingService.LogOperationStarted("BulkBannerDeletion", new { Count = banners.Count });

            // Delete all from database
            await _repo.DeleteRangeAsync(banners);
            await _repo.SaveAsync();

            _loggingService.LogOperationCompleted("BulkBannerDeletion", new { 
                Count = banners.Count, 
                BannerIds = banners.Select(b => b.Id).ToList() 
            });
            return true;
        }
    }
}
