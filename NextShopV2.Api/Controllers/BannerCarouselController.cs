using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextShopV2.Infrastructure.Persistence;
using NextShopV2.Domain.Entities.Marketing;
using NextShopV2.Api.Models;
using NextShopV2.Application.DTOs.Response;

namespace NextShopV2.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class BannerCarouselController : ControllerBase
	{
		private readonly AppDbContext _context;

		public BannerCarouselController(AppDbContext context)
		{
			_context = context;
		}

		// GET: api/BannerCarousel
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var banners = await _context.Advertisements.OrderBy(a => a.SortOrder).ToListAsync();
			return Ok(new ApiResponse { Success = true, Data = banners });
		}

		// GET: api/BannerCarousel/{id}
		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(Guid id)
		{
			var banner = await _context.Advertisements.FindAsync(id);
			if (banner == null)
				return NotFound(new ApiResponse { Success = false, Message = "Banner not found" });
			return Ok(new ApiResponse { Success = true, Data = banner });
		}

		// POST: api/BannerCarousel
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] Advertisement banner)
		{
			banner.AdvertisementId = Guid.NewGuid();
			_context.Advertisements.Add(banner);
			await _context.SaveChangesAsync();
			return CreatedAtAction(nameof(GetById), new { id = banner.AdvertisementId }, new ApiResponse { Success = true, Data = banner, Message = "Created successfully" });
		}

		// PUT: api/BannerCarousel/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> Update(Guid id, [FromBody] Advertisement banner)
		{
			if (id != banner.AdvertisementId)
				return BadRequest(new ApiResponse { Success = false, Message = "Id mismatch" });

			var exist = await _context.Advertisements.FindAsync(id);
			if (exist == null)
				return NotFound(new ApiResponse { Success = false, Message = "Banner not found" });

			_context.Entry(exist).CurrentValues.SetValues(banner);

			await _context.SaveChangesAsync();
			return Ok(new ApiResponse { Success = true, Message = "Updated successfully" });
		}

		// DELETE: api/BannerCarousel/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var banner = await _context.Advertisements.FindAsync(id);
			if (banner == null)
				return NotFound(new ApiResponse { Success = false, Message = "Banner not found" });
			_context.Advertisements.Remove(banner);
			await _context.SaveChangesAsync();
			return Ok(new ApiResponse { Success = true, Message = "Deleted successfully" });
		}

		// PATCH: api/BannerCarousel/{id}
		[HttpPatch("{id}")]
		public async Task<IActionResult> Patch(Guid id, [FromBody] AdvertisementPatchDto patchDto)
		{
			var banner = await _context.Advertisements.FindAsync(id);
			if (banner == null)
				return NotFound(new ApiResponse { Success = false, Message = "Banner not found" });

			patchDto.PatchTo(banner);

			await _context.SaveChangesAsync();
			return Ok(new ApiResponse { Success = true, Message = "Patched successfully", Data = banner });
		}
	}
}
