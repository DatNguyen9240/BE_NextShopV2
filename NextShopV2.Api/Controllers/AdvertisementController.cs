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
	[Route("api/Advertisements")]
	public class AdvertisementController : ControllerBase
	{
		private readonly IAdvertisementService _advertService;
		public AdvertisementController(IAdvertisementService advertisementService)
		{
			_advertService = advertisementService;
		}

		// GET: api/Advertisements
		[HttpGet]
		// Public endpoint - anyone can view advertisements
		public async Task<IActionResult> GetAll([FromQuery] string? type = null)
		{
			var ads = await _advertService.GetAllAsync(type);
			return ResponseHelper.Success(ads, "Advertisements retrieved successfully");
		}

// GET: api/Advertisements/{id}
	[HttpGet("{id}")]
	// Public endpoint - anyone can view a specific advertisement
	public async Task<IActionResult> GetById(Guid id)
	{
		var ad = await _advertService.GetByIdAsync(id);
		if (ad.IsNull())
			return ResponseHelper.NotFound("Advertisement not found");
		return ResponseHelper.Success(ad, "Advertisement retrieved successfully");
		}

// POST: api/Advertisements
	[HttpPost]
	[AdminOnly] // Only admin can create advertisements
	public async Task<IActionResult> Create([FromBody] AdvertisementRequestDto dto)
	{
		var ad = await _advertService.CreateAsync(dto);
		return CreatedAtAction(nameof(GetById), new { id = ad.Id }, new { Success = true, Message = "Created successfully", Data = ad });
		}

// PUT: api/Advertisements/{id}
	[HttpPut("{id}")]
	[AdminOnly] // Only admin can update advertisements
	public async Task<IActionResult> Update(Guid id, [FromBody] AdvertisementRequestDto dto)
	{
		var ok = await _advertService.UpdateAsync(id, dto);
			if (!ok) return ResponseHelper.BadRequest("Not found");
			return ResponseHelper.Success("Updated successfully");
		}

		// DELETE: api/Advertisements/{id}
		[HttpDelete("{id}")]
		[AdminOnly] // Only admin can delete advertisements
		public async Task<IActionResult> Delete(Guid id)
		{
			var ok = await _advertService.DeleteAsync(id);
			if (!ok) return ResponseHelper.NotFound("Advertisement not found");
			return ResponseHelper.Success("Deleted successfully");
		}

		// PATCH: api/Advertisements/{id}
		[HttpPatch("{id}")]
		[AdminOnly] // Only admin can patch banners
		public async Task<IActionResult> Patch(Guid id, [FromBody] AdvertisementRequestDto dto)
		{
			var ad = await _advertService.PatchAsync(id, dto);
			if (ad.IsNull()) return ResponseHelper.NotFound("Advertisement not found");
			return ResponseHelper.Success(ad, "Patched successfully");
		}
	}
}
