using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Shared.Helpers;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductLikeController : ControllerBase
    {
        private readonly IProductLikeService _productLikeService;

        public ProductLikeController(IProductLikeService productLikeService)
        {
            _productLikeService = productLikeService;
        }

        [HttpPost("like")]
        public async Task<IActionResult> LikeProduct([FromBody] LikeProductRequest request)
        {
            try
            {
                // In real app, get userId from JWT token
                var userId = Guid.NewGuid(); // Temporary - should come from authentication

                var result = await _productLikeService.LikeProductAsync(userId, request.ProductId);
                
                if (!result)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Product already liked by user"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Product liked successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("unlike")]
        public async Task<IActionResult> UnlikeProduct([FromBody] LikeProductRequest request)
        {
            try
            {
                // In real app, get userId from JWT token
                var userId = Guid.NewGuid(); // Temporary - should come from authentication

                var result = await _productLikeService.UnlikeProductAsync(userId, request.ProductId);
                
                if (!result)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Product not liked by user"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Product unliked successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleLike([FromBody] LikeProductRequest request)
        {
            try
            {
                // In real app, get userId from JWT token
                var userId = Guid.NewGuid(); // Temporary - should come from authentication

                var isLiked = await _productLikeService.ToggleLikeAsync(userId, request.ProductId);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = isLiked ? "Product liked" : "Product unliked",
                    Data = new { IsLiked = isLiked }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserLikes(Guid userId)
        {
            try
            {
                var likes = await _productLikeService.GetUserLikesAsync(userId);
                
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "User likes retrieved successfully",
                    Data = likes
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("product/{productId}/count")]
        public async Task<IActionResult> GetProductLikeCount(Guid productId)
        {
            try
            {
                var count = await _productLikeService.GetProductLikeCountAsync(productId);
                
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Product like count retrieved successfully",
                    Data = new { LikeCount = count }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("product/{productId}/is-liked-by/{userId}")]
        public async Task<IActionResult> IsLikedByUser(Guid productId, Guid userId)
        {
            try
            {
                var isLiked = await _productLikeService.IsLikedByUserAsync(userId, productId);
                
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Like status retrieved successfully",
                    Data = new { IsLiked = isLiked }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("user/{userId}/product-ids")]
        public async Task<IActionResult> GetLikedProductIds(Guid userId)
        {
            try
            {
                var productIds = await _productLikeService.GetLikedProductIdsAsync(userId);
                
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Liked product IDs retrieved successfully",
                    Data = productIds
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}