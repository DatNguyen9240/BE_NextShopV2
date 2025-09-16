using NextShopV2.Domain.Entities.Marketing;
using NextShopV2.Application.DTOs.Response;
namespace NextShopV2.Api.Models
{
    public static class AdvertisementPatchDtoExtensions
    {
        public static void PatchTo(this AdvertisementPatchDto dto, Advertisement entity)
        {
            if (dto.Title != null) entity.Title = dto.Title;
            if (dto.Description != null) entity.Description = dto.Description;
            if (dto.MediaUrl != null) entity.MediaUrl = dto.MediaUrl;
            if (dto.TargetUrl != null) entity.TargetUrl = dto.TargetUrl;
            if (dto.SortOrder.HasValue) entity.SortOrder = dto.SortOrder.Value;
            if (dto.StartDate.HasValue) entity.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue) entity.EndDate = dto.EndDate.Value;
            if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        }
    }
}