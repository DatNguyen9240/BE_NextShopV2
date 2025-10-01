using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace NextShopV2.Shared.Helpers
{
    /// <summary>
    /// JWT token generation and validation utilities
    /// </summary>
    public static class JwtHelper
    {
        /// <summary>
        /// Generate JWT token for user authentication
        /// </summary>
        /// <param name="key">Secret key for signing</param>
        /// <param name="userId">User ID</param>
        /// <param name="email">User email</param>
        /// <param name="expireHours">Token expiration in hours (default: 1)</param>
        /// <returns>JWT token string</returns>
        public static string GenerateToken(string key, Guid userId, string email, int expireHours = 1)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var keyBytes = Encoding.UTF8.GetBytes(key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Email, email),
                    new Claim("userId", userId.ToString()) // Add custom claim for easier access
                }),
                Expires = DateTime.UtcNow.AddHours(expireHours),
                Issuer = "NextShopAPI",
                Audience = "NextShopUsers",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}