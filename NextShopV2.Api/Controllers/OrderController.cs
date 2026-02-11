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
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IRedisCartService _cartService;

        public OrderController(IOrderService orderService, IRedisCartService cartService)
        {
            _orderService = orderService;
            _cartService = cartService;
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
            if (order is null)
                return ResponseHelper.NotFound("Order not found");

            // Check ownership: Admin can view all, User can only view their own
            var ownershipCheck = this.CheckResourceOwnership(order.UserId);
            if (ownershipCheck != null)
                return ownershipCheck;

            // Hide admin-only fields for non-admin users
            if (!this.IsAdmin())
            {
                order.AdminCancelReason = null;
                order.CancelledBy = null;
            }

            return ResponseHelper.Success(order);
        }

        [HttpGet("my-orders")]
        [Authenticated]
        public async Task<IActionResult> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
        {
            var (userId, error) = this.GetCurrentUserId();
            if (error != null)
                return ResponseHelper.Unauthorized(error);

            // If page/pageSize provided, return paged result (status optional)
            if (page > 0 && pageSize > 0)
            {
                var (items, total) = await _orderService.GetByUserIdPagedAsync(userId, page, pageSize, status);
                // Strip admin-only fields for non-admin users
                if (!this.IsAdmin())
                {
                    foreach (var it in items)
                    {
                        it.AdminCancelReason = null;
                        it.CancelledBy = null;
                    }
                }
                return ResponseHelper.Success(new { items, total, page, pageSize });
            }

            var orders = await _orderService.GetByUserIdAsync(userId);
            if (!this.IsAdmin())
            {
                foreach (var o in orders)
                {
                    o.AdminCancelReason = null;
                    o.CancelledBy = null;
                }
            }
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
            // Only admin should see admin-only fields
            if (!this.IsAdmin())
            {
                foreach (var o in orders)
                {
                    o.AdminCancelReason = null;
                    o.CancelledBy = null;
                }
            }
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
            Console.WriteLine($"Order created with OrderId: {result?.OrderId}");

            // If this order is a COD order, clear user's cart immediately on the backend
            try
            {
                if (!string.IsNullOrEmpty(request.PaymentMethod) && request.PaymentMethod.Equals("COD", StringComparison.OrdinalIgnoreCase))
                {
                    var cleared = await _cartService.ClearCartAsync(userId);
                    Console.WriteLine($"Cleared cart for user {userId} after COD order: {cleared}");
                }
            }
            catch (Exception ex)
            {
                // Log but do not fail order creation
                Console.WriteLine($"Failed to clear cart after COD order for user {userId}: {ex.Message}");
            }

            // Do not clear cart here for ONLINE payments; cart will be cleared when payment is confirmed by webhook.
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
        public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest? request = null)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order is null)
                return ResponseHelper.NotFound("Order not found");

            // For non-admin users, check ownership before cancelling
            if (!this.IsAdmin())
            {
                var ownershipCheck = this.CheckResourceOwnership(order.UserId);
                if (ownershipCheck != null)
                    return ownershipCheck;

                // Reject if non-admin tries to provide admin reason
                if (request?.AdminReason != null)
                    return ResponseHelper.BadRequest("Admin reason is reserved for admin users");
            }
            else
            {
                // Admin must provide an admin reason when cancelling
                if (string.IsNullOrWhiteSpace(request?.AdminReason))
                    return ResponseHelper.BadRequest("Admin reason is required when cancelling as admin");
            }

            var success = await _orderService.CancelOrderAsync(id, request?.Reason, request?.AdminReason, this.IsAdmin());
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