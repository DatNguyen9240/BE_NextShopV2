using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateInventory([FromBody] UpdateInventoryRequest request)
        {
            // Get current user (you might get this from JWT token or authentication)
            var currentUser = "Admin"; // TODO: Get from authentication context

            var result = await _inventoryService.UpdateInventoryAsync(
                request.VariantId,
                request.ChangeQty,
                request.Reason,
                currentUser);

            if (!result)
            {
                return BadRequest(new ApiResponse 
                { 
                    Success = false, 
                    Message = "Failed to update inventory. Insufficient stock or variant not found." 
                });
            }

            return Ok(new ApiResponse 
            { 
                Success = true, 
                Message = "Inventory updated successfully" 
            });
        }

        [HttpGet("history/{variantId}")]
        public async Task<IActionResult> GetInventoryHistory(Guid variantId)
        {
            var transactions = await _inventoryService.GetInventoryHistoryAsync(variantId);

            var response = transactions.Select(t => new InventoryTransactionResponse
            {
                TransactionId = t.TransactionId,
                VariantId = t.VariantId,
                VariantSKU = t.Variant?.SKU ?? "Unknown",
                ChangeQty = t.ChangeQty,
                Reason = t.Reason,
                CreatedAt = t.CreatedAt,
                CreatedBy = t.CreatedBy
            });

            return Ok(new ApiResponse 
            { 
                Success = true, 
                Data = response 
            });
        }

        [HttpGet("stock/{variantId}")]
        public async Task<IActionResult> GetCurrentStock(Guid variantId)
        {
            var stock = await _inventoryService.GetCurrentStockAsync(variantId);
            return Ok(new ApiResponse 
            { 
                Success = true, 
                Data = stock 
            });
        }

        [HttpGet("check-availability/{variantId}/{quantity}")]
        public async Task<IActionResult> CheckStockAvailability(Guid variantId, int quantity)
        {
            var isAvailable = await _inventoryService.CheckStockAvailabilityAsync(variantId, quantity);
            return Ok(new ApiResponse 
            { 
                Success = true, 
                Data = isAvailable 
            });
        }
    }
}