using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NextShopV2.Shared.Helpers;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace NextShopV2.Shared.Middlewares
{
    /// <summary>
    /// Global exception handling middleware
    /// </summary>
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
            
            int statusCode;
            object response;
            
            switch (exception)
            {
                case ArgumentNullException argNull:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    response = ResponseHelper.BadRequest($"Required parameter is missing: {argNull.ParamName}");
                    break;
                case ArgumentException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    response = ResponseHelper.BadRequest(exception.Message);
                    break;
                case UnauthorizedAccessException:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    response = ResponseHelper.Unauthorized(exception.Message);
                    break;
                case KeyNotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    response = ResponseHelper.NotFound(exception.Message);
                    break;
                case InvalidOperationException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    response = ResponseHelper.BadRequest(exception.Message);
                    break;
                case NotSupportedException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    response = ResponseHelper.BadRequest(exception.Message);
                    break;
                case TimeoutException:
                    statusCode = (int)HttpStatusCode.RequestTimeout;
                    response = ResponseHelper.Error("Request timeout");
                    break;
                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    response = ResponseHelper.Error("An unexpected error occurred");
                    break;
            }

            context.Response.StatusCode = statusCode;
            
            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}