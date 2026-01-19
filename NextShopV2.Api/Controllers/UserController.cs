using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.Interfaces;
using NextShopV2.Shared.Helpers;
using NextShopV2.Api.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepo;

        public UserController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        /// <summary>
        /// Get all users (Admin only)
        /// </summary>
        [HttpGet]
        [AdminOnly]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userRepo.GetAllAsync();
            var userResponses = users.Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.Phone,
                u.Role,
                u.EmailVerified,
                u.CreatedAt
            });

            return ResponseHelper.Success(userResponses);
        }

        /// <summary>
        /// Update user role (Admin only)
        /// </summary>
        [HttpPut("{userId}/role")]
        [AdminOnly]
        public async Task<IActionResult> UpdateRole(Guid userId, [FromBody] UpdateUserRoleRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.ValidationError(ModelState);

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return ResponseHelper.NotFound("User not found");

            // Validate role
            var validRoles = new[] { "User", "Shipper" };
            if (!validRoles.Contains(request.Role))
                return ResponseHelper.BadRequest("Invalid role. Valid roles: User, Shipper, Admin");

            user.Role = request.Role;
            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveAsync();

            return ResponseHelper.Success(new { user.Id, user.Role }, "User role updated successfully");
        }

        /// <summary>
        /// Get user by ID (Admin only)
        /// </summary>
        [HttpGet("{userId}")]
        [AdminOnly]
        public async Task<IActionResult> GetById(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return ResponseHelper.NotFound("User not found");

            return ResponseHelper.Success(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Phone,
                user.Role,
                user.EmailVerified,
                user.CreatedAt
            });
        }
    }

    public class UpdateUserRoleRequest
    {
        public string Role { get; set; } = null!;
    }
}
