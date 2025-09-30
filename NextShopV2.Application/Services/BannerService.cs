using NextShopV2.Application.Interfaces;
using NextShopV2.Domain.Entities.Marketing;
using NextShopV2.Application.DTOs.Request.CreateDto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace NextShopV2.Application.Services
{
    public class BannerService : IBannerService
    {
        private readonly IBannerRepository _repo;
        public BannerService(IBannerRepository repo)
        {
            _repo = repo;
        }
        public async Task<List<Advertisement>> GetAllAsync()
            => await _repo.GetAllAsync();
        public async Task<Advertisement?> GetByIdAsync(Guid id)
            => await _repo.GetByIdAsync(id);
    public async Task<Advertisement> CreateAsync(BannerRequestDto dto)
        {
            var banner = new Advertisement
            {
                Id = Guid.NewGuid(),
                PublicId = dto.PublicId ?? "",
                Title = dto.Title ?? string.Empty,
                ImageUrl = dto.ImageUrl ?? string.Empty,
                Type = dto.Type ?? string.Empty,
                SortOrder = dto.SortOrder ?? 0,
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(banner);
            await _repo.SaveAsync();
            return banner;
        }
    public async Task<bool> UpdateAsync(Guid id, BannerRequestDto dto)
        {
            var exist = await _repo.GetByIdAsync(id);
            if (exist == null) return false;
            // Update only allowed fields
            if (dto.PublicId != null) exist.PublicId = dto.PublicId;
            if (dto.Title != null) exist.Title = dto.Title;
            if (dto.ImageUrl != null) exist.ImageUrl = dto.ImageUrl;
            if (dto.Type != null) exist.Type = dto.Type;
            if (dto.SortOrder.HasValue) exist.SortOrder = dto.SortOrder.Value;
            // Do NOT update CreatedAt
            await _repo.UpdateAsync(exist);
            await _repo.SaveAsync();
            return true;
        }
        public async Task<bool> DeleteAsync(Guid id)
        {
            var banner = await _repo.GetByIdAsync(id);
            if (banner == null) return false;
            await _repo.DeleteAsync(banner);
            await _repo.SaveAsync();
            return true;
        }
        public async Task<Advertisement?> PatchAsync(Guid id, BannerRequestDto dto)
        {
            var banner = await _repo.GetByIdAsync(id);
            if (banner == null) return null;
            if (dto.PublicId != null) banner.PublicId = dto.PublicId;
            if (dto.Title != null) banner.Title = dto.Title;
            if (dto.ImageUrl != null) banner.ImageUrl = dto.ImageUrl;
            if (dto.Type != null) banner.Type = dto.Type;
            if (dto.SortOrder.HasValue) banner.SortOrder = dto.SortOrder.Value;
            await _repo.UpdateAsync(banner);
            await _repo.SaveAsync();
            return banner;
        }
    }
}
