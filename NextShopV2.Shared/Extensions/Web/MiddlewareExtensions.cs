using Microsoft.AspNetCore.Builder;
using NextShopV2.Shared.Middlewares;

namespace NextShopV2.Shared.Extensions.Web
{
    /// <summary>
    /// Extension methods for registering middleware
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Add global exception handling middleware
        /// </summary>
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionMiddleware>();
        }
    }
}