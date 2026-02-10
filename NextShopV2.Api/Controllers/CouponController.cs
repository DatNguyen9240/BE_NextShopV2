using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Shared.Helpers;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CouponController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var coupons = await _couponService.GetAllAsync();
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Coupons retrieved successfully",
                    Data = coupons
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
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var coupon = await _couponService.GetByIdAsync(id);
                if (coupon == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Coupon not found"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Coupon retrieved successfully",
                    Data = coupon
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

        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            try
            {
                var coupon = await _couponService.GetByCodeAsync(code);
                if (coupon == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Coupon not found"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Coupon retrieved successfully",
                    Data = coupon
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

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveCoupons()
        {
            try
            {
                var coupons = await _couponService.GetActiveCouponsAsync();
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Active coupons retrieved successfully",
                    Data = coupons
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
        public async Task<IActionResult> Create([FromBody] CreateCouponRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var coupon = await _couponService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = coupon.CouponId }, new ApiResponse
                {
                    Success = true,
                    Message = "Coupon created successfully",
                    Data = coupon
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCouponRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var coupon = await _couponService.UpdateAsync(id, request);
                if (coupon == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Coupon not found"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Coupon updated successfully",
                    Data = coupon
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
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _couponService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Coupon not found"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Coupon deleted successfully"
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

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateCoupon([FromBody] ApplyCouponRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var isValid = await _couponService.ValidateCouponAsync(request.CouponCode);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = isValid ? "Coupon is valid" : "Coupon is invalid or expired",
                    Data = new { IsValid = isValid }
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

        [HttpPost("calculate-discount")]
        public async Task<IActionResult> CalculateDiscount([FromBody] System.Text.Json.JsonElement request)
        {
            try
            {
                if (request.ValueKind != System.Text.Json.JsonValueKind.Object)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid payload"
                    });
                }

                // Try multiple casing variations
                string? couponCode = null;
                if (request.TryGetProperty("couponCode", out var p1) || request.TryGetProperty("CouponCode", out p1))
                {
                    if (p1.ValueKind == System.Text.Json.JsonValueKind.String)
                        couponCode = p1.GetString();
                }

                if (string.IsNullOrWhiteSpace(couponCode))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid coupon code"
                    });
                }

                decimal originalAmount = 0m;
                if (request.TryGetProperty("originalAmount", out var p2) || request.TryGetProperty("OriginalAmount", out p2))
                {
                    if (p2.ValueKind == System.Text.Json.JsonValueKind.Number && p2.TryGetDecimal(out var dec))
                    {
                        originalAmount = dec;
                    }
                    else if (p2.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        var s = p2.GetString();
                        if (!decimal.TryParse(s, out originalAmount))
                        {
                            return BadRequest(new ApiResponse { Success = false, Message = "Invalid originalAmount" });
                        }
                    }
                    else
                    {
                        return BadRequest(new ApiResponse { Success = false, Message = "Invalid originalAmount" });
                    }
                }
                else
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "Missing originalAmount" });
                }

                var discountAmount = await _couponService.CalculateDiscountAsync(couponCode, originalAmount);
                var finalAmount = originalAmount - discountAmount;

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Discount calculated successfully",
                    Data = new
                    {
                        OriginalAmount = originalAmount,
                        DiscountAmount = discountAmount,
                        FinalAmount = finalAmount
                    }
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

        [HttpGet("welcome-settings")]
        public async Task<IActionResult> GetWelcomeSettings()
        {
            try
            {
                var settings = await _couponService.GetWelcomeCouponSettingsAsync();
                if (settings == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Welcome coupon settings not found"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Welcome coupon settings retrieved successfully",
                    Data = settings
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

        [HttpPut("welcome-settings")]
        public async Task<IActionResult> UpdateWelcomeSettings([FromBody] UpdateWelcomeCouponSettingsRequest request)
        {
            try
            {
                var settings = await _couponService.UpdateWelcomeCouponSettingsAsync(request);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Welcome coupon settings updated successfully",
                    Data = settings
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