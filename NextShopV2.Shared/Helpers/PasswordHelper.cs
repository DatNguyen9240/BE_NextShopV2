using System;
using System.Security.Cryptography;
using System.Text;

namespace NextShopV2.Shared.Helpers
{
    /// <summary>
    /// Password hashing utilities for secure password management
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Hash password using SHA256 algorithm
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>Base64 encoded hash string</returns>
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Verify password against hash
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="hash">Stored hash to compare against</param>
        /// <returns>True if password matches hash</returns>
        public static bool VerifyPassword(string password, string hash)
        {
            var passwordHash = HashPassword(password);
            return passwordHash == hash;
        }
    }
}