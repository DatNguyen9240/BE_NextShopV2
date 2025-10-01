using Microsoft.AspNetCore.Mvc;

namespace NextShopV2.Shared.Helpers
{
    /// <summary>
    /// Authentication response DTO for shared use
    /// </summary>
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }

    /// <summary>
    /// Helper class for creating authentication-specific API responses
    /// </summary>
    public static class AuthResponseHelper
    {
        public static IActionResult Success(string message, string? accessToken = null, string? refreshToken = null)
        {
            if (accessToken == null && refreshToken == null)
                return new OkObjectResult(new ApiResponse { Success = true, Message = message ?? string.Empty });
            
            return new OkObjectResult(new AuthResponse 
            { 
                Success = true, 
                Message = message ?? string.Empty, 
                AccessToken = accessToken, 
                RefreshToken = refreshToken 
            });
        }

        public static IActionResult BadRequest(string message)
            => new BadRequestObjectResult(new ApiResponse { Success = false, Message = message });

        public static IActionResult Unauthorized(string message)
            => new ObjectResult(new ApiResponse { Success = false, Message = message }) { StatusCode = 401 };

        public static IActionResult NotFound(string message)
            => new NotFoundObjectResult(new ApiResponse { Success = false, Message = message });

        public static IActionResult ServerError(string message)
            => new ObjectResult(new ApiResponse { Success = false, Message = message }) { StatusCode = 500 };
    }
}