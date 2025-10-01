using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NextShopV2.Shared.Helpers
{
    /// <summary>
    /// Utility helpers for common operations across the application
    /// </summary>
    public static class CommonHelpers
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Generate a random string with specified length
        /// </summary>
        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var result = new StringBuilder();
            
            lock (_random)
            {
                for (int i = 0; i < length; i++)
                {
                    result.Append(chars[_random.Next(chars.Length)]);
                }
            }
            
            return result.ToString();
        }

        /// <summary>
        /// Generate a slug from a title (useful for SEO-friendly URLs)
        /// </summary>
        public static string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
                return string.Empty;

            return title.ToLowerInvariant()
                       .Replace(" ", "-")
                       .Replace("&", "and")
                       .Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
                       .Replace("é", "e").Replace("è", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
                       .Replace("í", "i").Replace("ì", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i")
                       .Replace("ó", "o").Replace("ò", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
                       .Replace("ú", "u").Replace("ù", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u")
                       .Replace("ý", "y").Replace("ỳ", "y").Replace("ỷ", "y").Replace("ỹ", "y").Replace("ỵ", "y")
                       .Replace("đ", "d");
        }

        /// <summary>
        /// Format currency for Vietnamese Dong
        /// </summary>
        public static string FormatCurrency(decimal amount)
        {
            return amount.ToString("N0") + " ₫";
        }

        /// <summary>
        /// Generate order code with prefix
        /// </summary>
        public static string GenerateOrderCode(string prefix = "DH")
        {
            var timestamp = DateTimeOffset.Now.ToString("yyyyMMdd");
            var random = GenerateRandomString(4);
            return $"{prefix}{timestamp}{random}"; // DH202410014X7B
        }

        /// <summary>
        /// Generate SKU with product prefix
        /// </summary>
        public static string GenerateSKU(string productPrefix, string? color = null, string? size = null)
        {
            var parts = new List<string> { productPrefix };
            
            if (!string.IsNullOrEmpty(color))
                parts.Add(color.Substring(0, Math.Min(2, color.Length)).ToUpper());
            
            if (!string.IsNullOrEmpty(size))
                parts.Add(size.ToUpper());
                
            parts.Add(GenerateRandomString(4));
            
            return string.Join("-", parts); // PRD-XA-XL-A7B9
        }

        /// <summary>
        /// Validate Vietnamese phone number
        /// </summary>
        public static bool IsValidVietnamesePhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return false;
                
            // Remove spaces and special characters
            phone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
            
            // Vietnamese phone patterns: 0xxx-xxx-xxx or +84xxx-xxx-xxx
            return phone.Length == 10 && phone.StartsWith("0") && phone.All(char.IsDigit) ||
                   phone.Length == 11 && phone.StartsWith("84") && phone.All(char.IsDigit);
        }

        /// <summary>
        /// Truncate text with ellipsis
        /// </summary>
        public static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;
                
            return text.Substring(0, maxLength - 3) + "...";
        }
    }
}