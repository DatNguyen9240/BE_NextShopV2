using Microsoft.AspNetCore.Mvc;
using NextShopV2.Domain.Entities.Marketing;
using NextShopV2.Application.Interfaces;
using NextShopV2.Shared.Helpers;
using NextShopV2.Api.Attributes;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/Announcements")]
    public class AnnouncementController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        // GET: api/Announcements/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var announcement = await _announcementService.GetActiveAsync();
            if (announcement == null)
            {
                return ResponseHelper.Success(null, "No active announcement");
            }
            return ResponseHelper.Success(announcement, "Active announcement retrieved successfully");
        }

        // GET: api/Announcements
        [HttpGet]
        [AdminOnly]
        public async Task<IActionResult> GetAll()
        {
            var announcements = await _announcementService.GetAllAsync();
            return ResponseHelper.Success(announcements, "Announcements retrieved successfully");
        }

        // POST: api/Announcements
        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> Create([FromBody] Announcement announcement)
        {
            var result = await _announcementService.CreateAsync(announcement);
            return ResponseHelper.Success(result, "Announcement created successfully");
        }

        // PUT: api/Announcements/{id}
        [HttpPut("{id}")]
        [AdminOnly]
        public async Task<IActionResult> Update(Guid id, [FromBody] Announcement announcement)
        {
            var success = await _announcementService.UpdateAsync(id, announcement);
            if (!success)
            {
                return ResponseHelper.NotFound("Announcement not found");
            }
            return ResponseHelper.Success(null, "Announcement updated successfully");
        }

        // DELETE: api/Announcements/{id}
        [HttpDelete("{id}")]
        [AdminOnly]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _announcementService.DeleteAsync(id);
            if (!success)
            {
                return ResponseHelper.NotFound("Announcement not found");
            }
            return ResponseHelper.Success(null, "Announcement deleted successfully");
        }
    }
}