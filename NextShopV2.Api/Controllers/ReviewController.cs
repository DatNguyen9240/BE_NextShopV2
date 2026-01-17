using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Api.Attributes;
using System.Security.Claims;

namespace NextShopV2.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReviews()
        {
            try
            {
                var reviews = await _reviewService.GetAllAsync();
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = reviews
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReview(Guid id)
        {
            try
            {
                var review = await _reviewService.GetByIdAsync(id);
                if (review == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Review not found"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = review
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

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetReviewsByProduct(Guid productId)
        {
            try
            {
                var reviews = await _reviewService.GetByProductIdAsync(productId);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = reviews
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

        [HttpGet("product/{productId}/stats")]
        public async Task<IActionResult> GetProductReviewStats(Guid productId)
        {
            try
            {
                var stats = await _reviewService.GetProductReviewStatsAsync(productId);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = stats
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

        [HttpGet("can-review/{productId}")]
        [Authorize]
        public async Task<IActionResult> CanReviewProduct(string productId)
        {
            try
            {
                if (!Guid.TryParse(productId, out var guid))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid productId format"
                    });
                }
                var userId = GetUserId();
                var canReview = await _reviewService.CanUserReviewProductAsync(userId, guid);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = canReview
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

        [HttpGet("my-reviews")]
        [Authorize]
        public async Task<IActionResult> GetMyReviews()
        {
            try
            {
                var userId = GetUserId();
                var reviews = await _reviewService.GetByUserIdAsync(userId);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = reviews
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
        {
            try
            {
                var userId = GetUserId();

                // Check if user can review this product
                var canReview = await _reviewService.CanUserReviewProductAsync(userId, request.ProductId);
                if (!canReview)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "You cannot review this product or have already reviewed it"
                    });
                }

                var review = await _reviewService.CreateAsync(userId, request);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = review,
                    Message = "Review created successfully"
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

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(Guid id, [FromBody] UpdateReviewRequest request)
        {
            try
            {
                // First, get the review to check ownership
                var existingReview = await _reviewService.GetByIdAsync(id);
                if (existingReview == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Review not found"
                    });
                }

                var userId = GetUserId();
                if (existingReview.UserId != userId)
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "You can only update your own reviews"
                    });
                }

                var review = await _reviewService.UpdateAsync(id, request);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = review,
                    Message = "Review updated successfully"
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

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(Guid id)
        {
            try
            {
                // First, get the review to check ownership
                var existingReview = await _reviewService.GetByIdAsync(id);
                if (existingReview == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Review not found"
                    });
                }

                var userId = GetUserId();
                if (existingReview.UserId != userId)
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "You can only delete your own reviews"
                    });
                }

                var result = await _reviewService.DeleteAsync(id);
                if (result)
                {
                    return Ok(new ApiResponse
                    {
                        Success = true,
                        Message = "Review deleted successfully"
                    });
                }

                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Failed to delete review"
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

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }
    }
}
