using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using NextShopV2.Shared.Helpers;

namespace NextShopV2.Shared.Extensions.Web
{
    /// <summary>
    /// Extension methods for authorization operations in ASP.NET Core controllers
    /// </summary>
    public static class AuthorizationExtensions
    {
        /// <summary>
        /// Get current user ID from JWT token
        /// </summary>
        public static (Guid userId, string? error) GetCurrentUserId(this ControllerBase controller)
        {
            var userIdClaim = controller.User.FindFirst(ClaimTypes.NameIdentifier) ?? controller.User.FindFirst("userId");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return (Guid.Empty, "Invalid user token");

            return (userId, null);
        }

        /// <summary>
        /// Check if current user is admin
        /// </summary>
        public static bool IsAdmin(this ControllerBase controller)
        {
            var userRole = controller.User.FindFirst(ClaimTypes.Role)?.Value;
            return userRole == "Admin";
        }

        /// <summary>
        /// Check if current user is shipper
        /// </summary>
        public static bool IsShipper(this ControllerBase controller)
        {
            var userRole = controller.User.FindFirst(ClaimTypes.Role)?.Value;
            return userRole == "Shipper";
        }

        /// <summary>
        /// Check if current user is admin or shipper
        /// </summary>
        public static bool IsAdminOrShipper(this ControllerBase controller)
        {
            var userRole = controller.User.FindFirst(ClaimTypes.Role)?.Value;
            return userRole == "Admin" || userRole == "Shipper";
        }



        /// <summary>
        /// Get current user role
        /// </summary>
        public static string? GetCurrentUserRole(this ControllerBase controller)
        {
            return controller.User.FindFirst(ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// Check if current user has specific role
        /// </summary>
        public static bool HasRole(this ControllerBase controller, string role)
        {
            var userRole = controller.User.FindFirst(ClaimTypes.Role)?.Value;
            return string.Equals(userRole, role, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if current user can access resource (Admin can access everything, User can only access their own)
        /// </summary>
        public static IActionResult? CheckResourceOwnership(this ControllerBase controller, Guid resourceUserId)
        {
            var userRole = controller.User.FindFirst(ClaimTypes.Role)?.Value;
            
            // Admin can access everything
            if (userRole == "Admin")
                return null; // No error, continue

            // Get current user ID
            var userIdClaim = controller.User.FindFirst(ClaimTypes.NameIdentifier) ?? controller.User.FindFirst("userId");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var currentUserId))
                return ResponseHelper.Unauthorized("Invalid user token");

            // Check if user owns the resource
            if (resourceUserId != currentUserId)
                return ResponseHelper.Unauthorized("You can only access your own resources");

            return null; // No error, user owns the resource
        }

        /// <summary>
        /// Check if current user can access user-specific resource (like /user/{userId})
        /// </summary>
        public static IActionResult? CheckUserAccess(this ControllerBase controller, Guid targetUserId)
        {
            var userRole = controller.User.FindFirst(ClaimTypes.Role)?.Value;
            
            // Admin can access any user's data
            if (userRole == "Admin")
                return null;

            // Get current user ID
            var userIdClaim = controller.User.FindFirst(ClaimTypes.NameIdentifier) ?? controller.User.FindFirst("userId");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var currentUserId))
                return ResponseHelper.Unauthorized("Invalid user token");

            // User can only access their own data
            if (targetUserId != currentUserId)
                return ResponseHelper.Unauthorized("You can only access your own data");

            return null;
        }
    }
}