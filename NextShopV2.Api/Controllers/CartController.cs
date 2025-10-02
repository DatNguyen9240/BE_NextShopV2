using Microsoft.AspNetCore.Mvc;
using NextShopV2.Shared.Interfaces;
using NextShopV2.Shared.Models;
using NextShopV2.Shared.Helpers;
using NextShopV2.Api.Attributes;
using NextShopV2.Shared.Extensions.Web;
using System;
using System.Threading.Tasks;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly IRedisCartService _cartService;

        public CartController(IRedisCartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        [Authenticated]
        public async Task<IActionResult> GetCart()
        {
            var (userId, error) = this.GetCurrentUserId();
            if (error != null)
                return ResponseHelper.Unauthorized(error);

            var cart = await _cartService.GetCartAsync(userId);
            return ResponseHelper.Success(cart);
        }

        [HttpPost("add")]
        [Authenticated]
        public async Task<IActionResult> AddToCart([FromBody] AddCartItemDto request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.ValidationError(ModelState);

            var (userId, error) = this.GetCurrentUserId();
            if (error != null)
                return ResponseHelper.Unauthorized(error);

            try
            {
                var cart = await _cartService.AddToCartAsync(userId, request);
                return ResponseHelper.Success(cart, "Item added to cart successfully");
            }
            catch (ArgumentException ex)
            {
                return ResponseHelper.BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return ResponseHelper.BadRequest(ex.Message);
            }
        }

        [HttpPut("items/{cartItemId}")]
        [Authenticated]
        public async Task<IActionResult> UpdateCartItem(Guid cartItemId, [FromBody] UpdateCartItemDto request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.ValidationError(ModelState);

            var (userId, error) = this.GetCurrentUserId();
            if (error != null)
                return ResponseHelper.Unauthorized(error);

            try
            {
                var cart = await _cartService.UpdateCartItemAsync(userId, cartItemId, request.Quantity);
                return ResponseHelper.Success(cart, "Cart item updated successfully");
            }
            catch (ArgumentException ex)
            {
                return ResponseHelper.BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return ResponseHelper.BadRequest(ex.Message);
            }
        }

        [HttpDelete("items/{cartItemId}")]
        [Authenticated]
        public async Task<IActionResult> RemoveFromCart(Guid cartItemId)
        {
            var (userId, error) = this.GetCurrentUserId();
            if (error != null)
                return ResponseHelper.Unauthorized(error);

            var success = await _cartService.RemoveFromCartAsync(userId, cartItemId);
            return success 
                ? ResponseHelper.Success("Item removed from cart successfully")
                : ResponseHelper.NotFound("Cart item not found");
        }

        [HttpDelete("clear")]
        [Authenticated]
        public async Task<IActionResult> ClearCart()
        {
            var (userId, error) = this.GetCurrentUserId();
            if (error != null)
                return ResponseHelper.Unauthorized(error);

            var success = await _cartService.ClearCartAsync(userId);
            return success 
                ? ResponseHelper.Success("Cart cleared successfully")
                : ResponseHelper.NotFound("Cart not found");
        }

        [HttpGet("count")]
        [Authenticated]
        public async Task<IActionResult> GetCartItemCount()
        {
            var (userId, error) = this.GetCurrentUserId();
            if (error != null)
                return ResponseHelper.Unauthorized(error);

            var count = await _cartService.GetCartItemCountAsync(userId);
            return ResponseHelper.Success(new { count });
        }
    }
}