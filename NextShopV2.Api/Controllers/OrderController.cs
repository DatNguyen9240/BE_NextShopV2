using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Api.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }



        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var orders = await _orderService.GetAllAsync();
                return ResponseHelper.Success(orders);
            }
            catch (Exception ex)
            {
                return ResponseHelper.Error(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(id);
                if (order == null)
                    return ResponseHelper.NotFound("Order not found");

                return ResponseHelper.Success(order);
            }
            catch (Exception ex)
            {
                return ResponseHelper.Error(ex.Message);
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            try
            {
                var orders = await _orderService.GetByUserIdAsync(userId);
                return ResponseHelper.Success(orders);
            }
            catch (Exception ex)
            {
                return ResponseHelper.Error(ex.Message);
            }
        }

        [HttpGet("my-orders")]
        [Authorize]
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                // Get userId from JWT token claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return ResponseHelper.Unauthorized("User not authenticated");

                var orders = await _orderService.GetByUserIdAsync(userId);
                return ResponseHelper.Success(orders);
            }
            catch (Exception ex)
            {
                return ResponseHelper.Error(ex.Message);
            }
        }

        [HttpGet("status/{status}")]
        [Authorize]
        public async Task<IActionResult> GetByStatus(string status)
        {
            try
            {
                var orders = await _orderService.GetByStatusAsync(status);
                return ResponseHelper.Success(orders);
            }
            catch (Exception ex)
            {
                return ResponseHelper.Error(ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return ResponseHelper.ValidationError(ModelState);

                // Get userId from JWT token claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return ResponseHelper.Unauthorized("User not authenticated");

                var result = await _orderService.CreateAsync(userId, request);
                return ResponseHelper.Created(result, "Order created successfully");
            }
            catch (ArgumentException ex)
            {
                return ResponseHelper.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return ResponseHelper.Error(ex.Message);
            }
        }

        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return ResponseHelper.ValidationError(ModelState);

                var success = await _orderService.UpdateStatusAsync(id, request);
                return success ? 
                    ResponseHelper.Success("Order status updated successfully") :
                    ResponseHelper.NotFound("Order not found");
            }
            catch (Exception ex)
            {
                return ResponseHelper.Error(ex.Message);
            }
        }

        [HttpPut("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            try
            {
                var success = await _orderService.CancelOrderAsync(id);
                return success ? 
                    ResponseHelper.Success("Order cancelled successfully") :
                    ResponseHelper.NotFound("Order not found or cannot be cancelled");
            }
            catch (InvalidOperationException ex)
            {
                return ResponseHelper.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return ResponseHelper.Error(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _orderService.DeleteAsync(id);
                return success ? 
                    ResponseHelper.Success("Order deleted successfully") :
                    ResponseHelper.NotFound("Order not found");
            }
            catch (Exception ex)
            {
                return ResponseHelper.Error(ex.Message);
            }
        }

        [HttpPost("calculate-total")]
        [Authorize]
        public async Task<IActionResult> CalculateTotal([FromBody] CreateOrderRequest request)
        {
            try
            {
                var total = await _orderService.CalculateOrderTotalAsync(request);
                return ResponseHelper.Success(new { total });
            }
            catch (Exception ex)
            {
                return ResponseHelper.Error(ex.Message);
            }
        }
    }
}