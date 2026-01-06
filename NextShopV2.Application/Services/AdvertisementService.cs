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
    public class AdvertisementService : IAdvertisementService
    {
        private readonly IAdvertisementRepository _repo;
        private readonly IOrderResolutionService _orderResolutionService;
        private readonly ILoggingService _loggingService;
        
        public AdvertisementService(IAdvertisementRepository repo, IOrderResolutionService orderResolutionService, ILoggingService loggingService)
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
    public async Task<Advertisement> CreateAsync(AdvertisementRequestDto dto)
        {
            var targetType = dto.Type ?? string.Empty;
            int resolvedSortOrder;

            if (dto.SortOrder.HasValue)
            {
                resolvedSortOrder = dto.SortOrder.Value;
                // Make room by incrementing existing items at or after requested position
                await _repo.IncrementSortOrdersFromAsync(targetType, resolvedSortOrder);
            }
            else
            {
                var existing = await _repo.GetByTypeAsync(targetType);
                resolvedSortOrder = existing.Any() ? existing.Max(b => b.SortOrder) + 1 : 1;
            }

            var banner = new Advertisement
            {
                Id = Guid.NewGuid(),
                PublicId = dto.PublicId ?? "",
                Title = dto.Title ?? string.Empty,
                ImageUrl = dto.ImageUrl ?? string.Empty,
                Type = dto.Type ?? string.Empty,
                SortOrder = resolvedSortOrder,
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(banner);
            await _repo.SaveAsync();
            return banner;
        }
    public async Task<bool> UpdateAsync(Guid id, AdvertisementRequestDto dto)
        {
            var exist = await _repo.GetByIdAsync(id);
            if (exist.IsNull()) return false;

            // Auto-resolve SortOrder conflict for update by shifting ranges in DB
            var resolvedSortOrder = exist!.SortOrder;
            if (dto.SortOrder.HasValue)
            {
                var newOrder = dto.SortOrder.Value;
                var oldOrder = exist.SortOrder;
                var finalType = dto.Type ?? exist.Type ?? string.Empty;
                var oldType = exist.Type ?? string.Empty;

                if (!string.Equals(finalType, oldType, StringComparison.OrdinalIgnoreCase))
                {
                    // Moving to a different type: remove gap from old type and insert into new type
                    await _repo.DecrementSortOrdersAfterAsync(oldType, oldOrder);
                    await _repo.IncrementSortOrdersFromAsync(finalType, newOrder);
                }
                else
                {
                    // Same type: shift the range between old and new
                    if (newOrder < oldOrder)
                    {
                        // make space between newOrder..oldOrder-1 => +1
                        await _repo.ShiftSortOrdersInRangeAsync(oldType, newOrder, oldOrder - 1, 1);
                    }
                    else if (newOrder > oldOrder)
                    {
                        // move down: decrement range oldOrder+1..newOrder => -1
                        await _repo.ShiftSortOrdersInRangeAsync(oldType, oldOrder + 1, newOrder, -1);
                    }
                }

                resolvedSortOrder = newOrder;
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
                _loggingService.LogEntityNotFound("Advertisement", id);
                return false;
            }
            
            // Delete from database
            await _repo.DeleteAsync(banner!);
            await _repo.SaveAsync();

            // Close gap in sort orders for the same type
            await _repo.DecrementSortOrdersAfterAsync(banner!.Type ?? string.Empty, banner.SortOrder);

            _loggingService.LogEntityDeleted("Advertisement", id);
            return true;
        }
        public async Task<Advertisement?> PatchAsync(Guid id, AdvertisementRequestDto dto)
        {
            var banner = await _repo.GetByIdAsync(id);
            if (banner.IsNull()) return null;

            // Auto-resolve SortOrder conflict for patch by shifting ranges in DB
            var resolvedSortOrder = banner!.SortOrder;
            if (dto.SortOrder.HasValue)
            {
                var newOrder = dto.SortOrder.Value;
                var oldOrder = banner.SortOrder;
                var finalType = dto.Type ?? banner.Type ?? string.Empty;
                var oldType = banner.Type ?? string.Empty;

                if (!string.Equals(finalType, oldType, StringComparison.OrdinalIgnoreCase))
                {
                    await _repo.DecrementSortOrdersAfterAsync(oldType, oldOrder);
                    await _repo.IncrementSortOrdersFromAsync(finalType, newOrder);
                }
                else
                {
                    if (newOrder < oldOrder)
                    {
                        await _repo.ShiftSortOrdersInRangeAsync(oldType, newOrder, oldOrder - 1, 1);
                    }
                    else if (newOrder > oldOrder)
                    {
                        await _repo.ShiftSortOrdersInRangeAsync(oldType, oldOrder + 1, newOrder, -1);
                    }
                }

                resolvedSortOrder = newOrder;
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
        /// Get advertisements grouped by their Type field
        /// </summary>
        public async Task<Dictionary<string, List<Advertisement>>> GetGroupedAsync()
        {
            var all = await _repo.GetAllAsync();

            // Group by type, sort within each group by SortOrder then Id, and normalize SortOrder to 1..n in the returned payload
            var grouped = all
                .GroupBy(a => string.IsNullOrWhiteSpace(a.Type) ? "default" : a.Type)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(a => a.SortOrder).ThenBy(a => a.Id)
                          .Select((a, idx) => new Advertisement
                          {
                              Id = a.Id,
                              PublicId = a.PublicId,
                              Title = a.Title,
                              ImageUrl = a.ImageUrl,
                              Type = a.Type,
                              SortOrder = idx + 1, // normalized
                              CreatedAt = a.CreatedAt
                          }).ToList()
                );

            return grouped;
        }

        /// <summary>
        /// Delete multiple banners - Example usage of SafeForEachAsync
        /// </summary>
        public async Task<bool> DeleteMultipleAdvertisementsAsync(List<Guid> advertisementIds)
        {
            if (advertisementIds.IsNullOrEmpty())
            {
                _loggingService.LogValidationFailure("BulkAdvertisementDeletion", "Empty or null advertisement IDs");
                return false;
            }

            var ads = await _repo.GetByIdsAsync(advertisementIds);
            
            if (ads.IsNullOrEmpty())
            {
                _loggingService.LogValidationFailure("BulkAdvertisementDeletion", new { AdvertisementIds = advertisementIds, Reason = "No advertisements found" });
                return false;
            }

            _loggingService.LogOperationStarted("BulkAdvertisementDeletion", new { Count = ads.Count });

            // Delete all from database
            await _repo.DeleteRangeAsync(ads);
            await _repo.SaveAsync();

            _loggingService.LogOperationCompleted("BulkAdvertisementDeletion", new { Count = ads.Count, AdvertisementIds = ads.Select(b => b.Id).ToList() });
            return true;
        }
    }
}


