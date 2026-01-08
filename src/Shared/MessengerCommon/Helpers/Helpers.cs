using System;
using System.Security.Cryptography;

namespace MessengerCommon.Helpers
{
    /// <summary>
    /// Helper class for cryptographic operations
    /// </summary>
    public static class CryptoHelper
    {
        /// <summary>
        /// Generate cryptographically secure random bytes
        /// </summary>
        public static byte[] GenerateRandomBytes(int length)
        {
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            return bytes;
        }

        /// <summary>
        /// Generate a secure salt for password hashing
        /// </summary>
        public static byte[] GenerateSalt()
        {
            return GenerateRandomBytes(32); // 256 bits
        }

        /// <summary>
        /// Securely compare two byte arrays (constant-time)
        /// </summary>
        public static bool SecureEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            return CryptographicOperations.FixedTimeEquals(a, b);
        }

        /// <summary>
        /// Securely zero memory
        /// </summary>
        public static void ZeroMemory(byte[] data)
        {
            if (data != null && data.Length > 0)
            {
                CryptographicOperations.ZeroMemory(data);
            }
        }
    }

    /// <summary>
    /// Helper class for validation
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validate email format
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate password strength
        /// </summary>
        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Minimum 8 characters, at least one uppercase, one lowercase, one number, one special character
            return password.Length >= 8 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit) &&
                   password.Any(ch => !char.IsLetterOrDigit(ch));
        }

        /// <summary>
        /// Validate username format
        /// </summary>
        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            // 3-20 characters, alphanumeric and underscores only
            return username.Length >= 3 &&
                   username.Length <= 20 &&
                   username.All(ch => char.IsLetterOrDigit(ch) || ch == '_');
        }
    }
}
