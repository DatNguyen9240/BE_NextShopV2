using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.DTOs.Response;

namespace NextShopV2.Api.Helpers
{
    public static class ResponseHelper
    {
        public static IActionResult Success(string message, object? data = null)
        {
            if (data == null)
                return new OkObjectResult(new { Success = true, Message = message ?? string.Empty });
            return new OkObjectResult(new ApiResponse { Success = true, Message = message ?? string.Empty, Data = data });
        }

        public static IActionResult BadRequest(string message)
            => new BadRequestObjectResult(new { Success = false, Message = message });

        public static IActionResult Unauthorized(string message)
            => new UnauthorizedObjectResult(new { Success = false, Message = message });

        public static IActionResult NotFound(string message)
            => new NotFoundObjectResult(new { Success = false, Message = message });

        public static IActionResult ServerError(string message)
            => new ObjectResult(new { Success = false, Message = message }) { StatusCode = 500 };
    }
}