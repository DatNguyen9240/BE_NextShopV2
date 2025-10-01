using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Shared.Helpers;
using NextShopV2.Shared.Extensions;
using NextShopV2.Api.Attributes;
using NextShopV2.Shared.Extensions.Web;
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
        [AdminOnly]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllAsync();
            return ResponseHelper.Success(orders);
        }

        [HttpGet("{id}")]
        [AdminOrUser]
        public async Task<IActionResult> GetById(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order.IsNull())
                return ResponseHelper.NotFound("Order not found");

            // Check ownership: Admin can view all, User can only view their own
            var ownershipCheck = this.CheckResourceOwnership(order.UserId);
            if (ownershipCheck != null)
                return ownershipCheck;

            return ResponseHelper.Success(order);
        }

        [HttpGet("my-orders")]
        [Authenticated]
        public async Task<IActionResult> GetMyOrders()
        {
            var (userId, error) = this.GetCurrentUserId();
            if (error != null)
                return ResponseHelper.Unauthorized(error);

            var orders = await _orderService.GetByUserIdAsync(userId);
            return ResponseHelper.Success(orders);
        }

        [HttpGet("user/{userId}")]
        [AdminOrUser]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            // Check user access: Admin can view any user's orders, User can only view their own
            var userCheck = this.CheckUserAccess(userId);
            if (userCheck != null)
                return userCheck;

            var orders = await _orderService.GetByUserIdAsync(userId);
            return ResponseHelper.Success(orders);
        }



        [HttpGet("status/{status}")]
        [AdminOnly]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var orders = await _orderService.GetByStatusAsync(status);
            return ResponseHelper.Success(orders);
        }

        [HttpPost]
        [Authenticated]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.ValidationError(ModelState);

            var (userId, error) = this.GetCurrentUserId();
            if (error != null)
                return ResponseHelper.Unauthorized(error);

            var result = await _orderService.CreateAsync(userId, request);
            return ResponseHelper.Created(result, "Order created successfully");
        }

        [HttpPut("{id}/status")]
        [AdminOnly]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.ValidationError(ModelState);

            var success = await _orderService.UpdateStatusAsync(id, request);
            return success ? 
                ResponseHelper.Success("Order status updated successfully") :
                ResponseHelper.NotFound("Order not found");
        }

        [HttpPut("{id}/cancel")]
        [AdminOrUser]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            // For non-admin users, check ownership before cancelling
            if (!this.IsAdmin())
            {
                var order = await _orderService.GetByIdAsync(id);
                if (order.IsNull())
                    return ResponseHelper.NotFound("Order not found");

                var ownershipCheck = this.CheckResourceOwnership(order.UserId);
                if (ownershipCheck != null)
                    return ownershipCheck;
            }

            var success = await _orderService.CancelOrderAsync(id);
            return success ? 
                ResponseHelper.Success("Order cancelled successfully") :
                ResponseHelper.NotFound("Order not found or cannot be cancelled");
        }

        [HttpDelete("{id}")]
        [AdminOnly]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _orderService.DeleteAsync(id);
            return success ? 
                ResponseHelper.Success("Order deleted successfully") :
                ResponseHelper.NotFound("Order not found");
        }

        [HttpPost("calculate-total")]
        [Authenticated]
        public async Task<IActionResult> CalculateTotal([FromBody] CreateOrderRequest request)
        {
            var total = await _orderService.CalculateOrderTotalAsync(request);
            return ResponseHelper.Success(new { total });
        }
    }
}