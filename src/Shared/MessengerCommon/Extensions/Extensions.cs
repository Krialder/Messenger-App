using System;
using System.Security.Cryptography;

namespace MessengerCommon.Extensions
{
    /// <summary>
    /// Extension methods for string manipulation
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Convert string to Base64
        /// </summary>
        public static string ToBase64(this string input)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Convert Base64 string to plain string
        /// </summary>
        public static string FromBase64(this string input)
        {
            var bytes = Convert.FromBase64String(input);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Check if string is null or whitespace
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string? input)
        {
            return string.IsNullOrWhiteSpace(input);
        }

        /// <summary>
        /// Truncate string to specified length
        /// </summary>
        public static string Truncate(this string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input.Length <= maxLength ? input : input.Substring(0, maxLength);
        }
    }

    /// <summary>
    /// Extension methods for byte arrays
    /// </summary>
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Convert byte array to hex string
        /// </summary>
        public static string ToHexString(this byte[] bytes)
        {
            return Convert.ToHexString(bytes);
        }

        /// <summary>
        /// Securely zero out byte array
        /// </summary>
        public static void SecureZero(this byte[] bytes)
        {
            if (bytes != null && bytes.Length > 0)
            {
                CryptographicOperations.ZeroMemory(bytes);
            }
        }

        /// <summary>
        /// Convert byte array to Base64
        /// </summary>
        public static string ToBase64String(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }
    }

    /// <summary>
    /// Extension methods for DateTime
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Convert DateTime to Unix timestamp
        /// </summary>
        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
        }

        /// <summary>
        /// Convert Unix timestamp to DateTime
        /// </summary>
        public static DateTime FromUnixTimestamp(this long timestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        }

        /// <summary>
        /// Check if DateTime is within the last N minutes
        /// </summary>
        public static bool IsWithinLast(this DateTime dateTime, int minutes)
        {
            return dateTime >= DateTime.UtcNow.AddMinutes(-minutes);
        }
    }
}
