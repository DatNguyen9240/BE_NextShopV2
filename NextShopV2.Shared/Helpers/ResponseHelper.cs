using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NextShopV2.Shared.Helpers
{
    /// <summary>
    /// Standard API response structure
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Data { get; set; }
    }

    /// <summary>
    /// Helper class for creating standardized API responses
    /// </summary>
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
            return Success(null, message);
        }

        public static IActionResult Created(object? data = null, string message = "Created successfully")
        {
            return new CreatedResult(string.Empty, new ApiResponse 
            { 
                Success = true, 
                Message = message, 
                Data = data 
            });
        }

        public static IActionResult BadRequest(string message = "Bad request")
        {
            return new BadRequestObjectResult(new ApiResponse 
            { 
                Success = false, 
                Message = message 
            });
        }

        public static IActionResult NotFound(string message = "Resource not found")
        {
            return new NotFoundObjectResult(new ApiResponse 
            { 
                Success = false, 
                Message = message 
            });
        }

        public static IActionResult Unauthorized(string message = "Unauthorized access")
        {
            return new ObjectResult(new ApiResponse 
            { 
                Success = false, 
                Message = message 
            }) { StatusCode = 401 };
        }

        public static IActionResult ValidationError(ModelStateDictionary modelState)
        {
            var errors = modelState
                .SelectMany(x => x.Value?.Errors ?? new ModelErrorCollection())
                .Select(x => x.ErrorMessage)
                .ToArray();

            return new BadRequestObjectResult(new ApiResponse 
            { 
                Success = false, 
                Message = "Validation failed", 
                Data = errors 
            });
        }

        public static IActionResult InternalServerError(string message = "Internal server error")
        {
            return new ObjectResult(new ApiResponse 
            { 
                Success = false, 
                Message = message 
            })
            {
                StatusCode = 500
            };
        }

        public static IActionResult Conflict(string message = "Conflict")
        {
            return new ObjectResult(new ApiResponse
            {
                Success = false,
                Message = message
            }) { StatusCode = 409 };
        }

        public static ApiResponse Error(string message = "An error occurred")
        {
            return new ApiResponse 
            { 
                Success = false, 
                Message = message 
            };
        }
    }
}