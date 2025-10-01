using NextShopV2.Api.Helpers;
using System.Net;
using System.Text.Json;

namespace NextShopV2.Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = exception switch
            {
                ArgumentException => new { 
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Response = ResponseHelper.BadRequest(exception.Message)
                },
                UnauthorizedAccessException => new {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Response = ResponseHelper.Unauthorized(exception.Message)
                },
                KeyNotFoundException => new {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Response = ResponseHelper.NotFound(exception.Message)
                },
                InvalidOperationException => new {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Response = ResponseHelper.BadRequest(exception.Message)
                },
                _ => new {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Response = ResponseHelper.Error("An unexpected error occurred")
                }
            };

            context.Response.StatusCode = response.StatusCode;
            
            var jsonResponse = JsonSerializer.Serialize(response.Response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}