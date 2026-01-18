using Microsoft.AspNetCore.Mvc;
using NextShopV2.Domain.Entities.Marketing;
using NextShopV2.Application.Interfaces;
using NextShopV2.Shared.Helpers;
using NextShopV2.Api.Attributes;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/FooterInfo")]
    public class FooterInfoController : ControllerBase
    {
        private readonly IFooterInfoService _footerInfoService;

        public FooterInfoController(IFooterInfoService footerInfoService)
        {
            _footerInfoService = footerInfoService;
        }

        // GET: api/FooterInfo
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var footerInfo = await _footerInfoService.GetAsync();
            if (footerInfo == null)
            {
                return ResponseHelper.Success(null, "No footer info found");
            }
            return ResponseHelper.Success(footerInfo, "Footer info retrieved successfully");
        }

        // POST: api/FooterInfo
        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> Update([FromBody] FooterInfo footerInfo)
        {
            if (footerInfo == null)
            {
                return ResponseHelper.BadRequest("Footer info is required");
            }
            var result = await _footerInfoService.UpdateAsync(footerInfo);
            return ResponseHelper.Success(result, "Footer info updated successfully");
        }
    }
}