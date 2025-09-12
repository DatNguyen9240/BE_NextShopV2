using Microsoft.AspNetCore.Mvc;
using NextShopV2.Api.Models;

namespace NextShopV2.Api.Helpers
{
    public static class ResponseHelper
    {
        public static IActionResult Success(string message, object? data = null)
            => new OkObjectResult(new ApiResponse { Success = true, Message = message ?? string.Empty, Data = data });

        public static IActionResult BadRequest(string message)
            => new BadRequestObjectResult(new AuthResponse { Success = false, Message = message });

        public static IActionResult Unauthorized(string message)
            => new UnauthorizedObjectResult(new AuthResponse { Success = false, Message = message });

        public static IActionResult NotFound(string message)
            => new NotFoundObjectResult(new AuthResponse { Success = false, Message = message });

        public static IActionResult ServerError(string message)
            => new ObjectResult(new AuthResponse { Success = false, Message = message }) { StatusCode = 500 };
    }
}