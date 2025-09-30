using Microsoft.AspNetCore.Mvc;
using NextShopV2.Domain.Entities.Marketing;
using NextShopV2.Application.DTOs.Request.CreateDto;
using NextShopV2.Application.Interfaces;
using NextShopV2.Api.Helpers;

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
		public async Task<IActionResult> GetAll()
		{
			var banners = await _bannerService.GetAllAsync();
			return ResponseHelper.Success("", banners);
		}

		// GET: api/BannerCarousel/{id}
		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(Guid id)
		{
			var banner = await _bannerService.GetByIdAsync(id);
			if (banner == null)
				return ResponseHelper.NotFound("Banner not found");
			return ResponseHelper.Success("", banner);
		}

		// POST: api/BannerCarousel
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] BannerRequestDto dto)
		{
			var banner = await _bannerService.CreateAsync(dto);
			return CreatedAtAction(nameof(GetById), new { id = banner.Id }, new { Success = true, Message = "Created successfully", Data = banner });
		}

		// PUT: api/BannerCarousel/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> Update(Guid id, [FromBody] BannerRequestDto dto)
		{
			var ok = await _bannerService.UpdateAsync(id, dto);
			if (!ok) return ResponseHelper.BadRequest("Not found");
			return ResponseHelper.Success("Updated successfully");
		}

		// DELETE: api/BannerCarousel/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var ok = await _bannerService.DeleteAsync(id);
			if (!ok) return ResponseHelper.NotFound("Banner not found");
			return ResponseHelper.Success("Deleted successfully");
		}

		// PATCH: api/BannerCarousel/{id}
		[HttpPatch("{id}")]
		public async Task<IActionResult> Patch(Guid id, [FromBody] BannerRequestDto dto)
		{
			var banner = await _bannerService.PatchAsync(id, dto);
			if (banner == null) return ResponseHelper.NotFound("Banner not found");
			return ResponseHelper.Success("Patched successfully", banner);
		}
	}
}
