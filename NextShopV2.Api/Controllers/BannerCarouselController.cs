using Microsoft.AspNetCore.Mvc;
using NextShopV2.Domain.Entities.Marketing;
using NextShopV2.Application.DTOs.Request.CreateDto;
using NextShopV2.Application.Interfaces;
using NextShopV2.Shared.Helpers;
using NextShopV2.Shared.Extensions;
using NextShopV2.Api.Attributes;

namespace NextShopV2.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class BannerCarouselController : ControllerBase
	{
		private readonly IBannerService _bannerService;
		public BannerCarouselController(IBannerService bannerService)
		{
			_bannerService = bannerService;
		}

		// GET: api/BannerCarousel
		[HttpGet]
		// Public endpoint - anyone can view banners
		public async Task<IActionResult> GetAll([FromQuery] string? type = null)
		{
			var banners = await _bannerService.GetAllAsync(type);
			return ResponseHelper.Success(banners, "Banners retrieved successfully");
		}

		// GET: api/BannerCarousel/{id}
		[HttpGet("{id}")]
		// Public endpoint - anyone can view a specific banner
		public async Task<IActionResult> GetById(Guid id)
		{
			var banner = await _bannerService.GetByIdAsync(id);
			if (banner.IsNull())
				return ResponseHelper.NotFound("Banner not found");
			return ResponseHelper.Success(banner, "Banner retrieved successfully");
		}

		// POST: api/BannerCarousel
		[HttpPost]
		[AdminOnly] // Only admin can create banners
		public async Task<IActionResult> Create([FromBody] BannerRequestDto dto)
		{
			var banner = await _bannerService.CreateAsync(dto);
			return CreatedAtAction(nameof(GetById), new { id = banner.Id }, new { Success = true, Message = "Created successfully", Data = banner });
		}

		// PUT: api/BannerCarousel/{id}
		[HttpPut("{id}")]
		[AdminOnly] // Only admin can update banners
		public async Task<IActionResult> Update(Guid id, [FromBody] BannerRequestDto dto)
		{
			var ok = await _bannerService.UpdateAsync(id, dto);
			if (!ok) return ResponseHelper.BadRequest("Not found");
			return ResponseHelper.Success("Updated successfully");
		}

		// DELETE: api/BannerCarousel/{id}
		[HttpDelete("{id}")]
		[AdminOnly] // Only admin can delete banners
		public async Task<IActionResult> Delete(Guid id)
		{
			var ok = await _bannerService.DeleteAsync(id);
			if (!ok) return ResponseHelper.NotFound("Banner not found");
			return ResponseHelper.Success("Deleted successfully");
		}

		// PATCH: api/BannerCarousel/{id}
		[HttpPatch("{id}")]
		[AdminOnly] // Only admin can patch banners
		public async Task<IActionResult> Patch(Guid id, [FromBody] BannerRequestDto dto)
		{
			var banner = await _bannerService.PatchAsync(id, dto);
			if (banner.IsNull()) return ResponseHelper.NotFound("Banner not found");
			return ResponseHelper.Success(banner, "Patched successfully");
		}
	}
}
