using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NextShopV2.Application.DTOs.Response;

namespace NextShopV2.Api.Helpers
{
    public static class ResponseHelper
    {
        public static IActionResult Success(object? data = null, string message = "Success")
        {
            return new OkObjectResult(new ApiResponse 
            { 
                Success = true, 
                Message = message, 
                Data = data 
            });
        }

        public static IActionResult Success(string message)
        {
            return new OkObjectResult(new { Success = true, Message = message });
        }

        public static IActionResult Created(object? data = null, string message = "Created successfully")
        {
            return new ObjectResult(new ApiResponse 
            { 
                Success = true, 
                Message = message, 
                Data = data 
            }) { StatusCode = 201 };
        }

        public static IActionResult BadRequest(string message)
            => new BadRequestObjectResult(new { Success = false, Message = message });

        public static IActionResult ValidationError(ModelStateDictionary modelState)
        {
            var errors = modelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return new BadRequestObjectResult(new 
            { 
                Success = false, 
                Message = "Validation failed", 
                Errors = errors 
            });
        }

        public static IActionResult Unauthorized(string message)
            => new UnauthorizedObjectResult(new { Success = false, Message = message });

        public static IActionResult NotFound(string message)
            => new NotFoundObjectResult(new { Success = false, Message = message });

        public static IActionResult Error(string message)
            => new ObjectResult(new { Success = false, Message = message }) { StatusCode = 500 };

        public static IActionResult ServerError(string message)
            => new ObjectResult(new { Success = false, Message = message }) { StatusCode = 500 };
    }
}