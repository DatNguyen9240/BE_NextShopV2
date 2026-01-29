using Microsoft.AspNetCore.Mvc;
using NextShopV2.Domain.Entities.Marketing;
using NextShopV2.Application.Interfaces;
using NextShopV2.Shared.Helpers;
using NextShopV2.Api.Attributes;
using System.Linq;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/MarketingElements")]
    public class MarketingElementController : ControllerBase
    {
        private readonly IMarketingElementService _marketingElementService;

        public MarketingElementController(IMarketingElementService marketingElementService)
        {
            _marketingElementService = marketingElementService;
        }

        // GET: api/MarketingElements/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var marketingElement = await _marketingElementService.GetActiveAsync();
            if (marketingElement == null)
            {
                return ResponseHelper.Success(null, "No active marketing element");
            }
            return ResponseHelper.Success(marketingElement, "Active marketing element retrieved successfully");
        }

        // GET: api/MarketingElements/public
        [HttpGet("public")]
        public async Task<IActionResult> GetPublic([FromQuery] string? name = null)
        {
            var marketingElements = await _marketingElementService.GetAllAsync();
            if (!string.IsNullOrEmpty(name))
            {
                marketingElements = marketingElements.Where(m => m.Name == name).ToList();
            }
            return ResponseHelper.Success(marketingElements, "Marketing elements retrieved successfully");
        }

        // GET: api/MarketingElements
        [HttpGet]
        [AdminOnly]
        public async Task<IActionResult> GetAll([FromQuery] string? name = null)
        {
            var marketingElements = await _marketingElementService.GetAllAsync();
            if (!string.IsNullOrEmpty(name))
            {
                marketingElements = marketingElements.Where(m => m.Name == name).ToList();
            }
            return ResponseHelper.Success(marketingElements, "Marketing elements retrieved successfully");
        }

        // POST: api/MarketingElements
        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> Create([FromBody] MarketingElement marketingElement)
        {
            var result = await _marketingElementService.CreateAsync(marketingElement);
            return ResponseHelper.Success(result, "Marketing element created successfully");
        }

        // PUT: api/MarketingElements/{id}
        [HttpPut("{id}")]
        [AdminOnly]
        public async Task<IActionResult> Update(Guid id, [FromBody] MarketingElement marketingElement)
        {
            var success = await _marketingElementService.UpdateAsync(id, marketingElement);
            if (!success)
            {
                return ResponseHelper.NotFound("Marketing element not found");
            }
            return ResponseHelper.Success(null, "Marketing element updated successfully");
        }

        // DELETE: api/MarketingElements/{id}
        [HttpDelete("{id}")]
        [AdminOnly]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _marketingElementService.DeleteAsync(id);
            if (!success)
            {
                return ResponseHelper.NotFound("Marketing element not found");
            }
            return ResponseHelper.Success(null, "Marketing element deleted successfully");
        }
    }
}