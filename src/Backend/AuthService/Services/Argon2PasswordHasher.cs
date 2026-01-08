using Konscious.Security.Cryptography;
using MessengerContracts.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Services;

/// <summary>
/// Password hasher using Argon2id (OWASP recommended)
/// </summary>
public class Argon2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 3;
    private const int MemorySize = 65536; // 64 MB
    private const int DegreeOfParallelism = 1;

    /// <summary>
    /// Hash a password using Argon2id
    /// </summary>
    public string HashPassword(string password)
    {
        // Generate random salt
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Hash password
        var hash = HashPasswordInternal(password, salt);

        // Combine salt and hash for storage: salt:hash (both base64)
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verify password against stored hash
    /// </summary>
    public bool VerifyPassword(string password, string storedHash)
    {
        try
        {
            // Split stored hash into salt and hash
            var parts = storedHash.Split(':');
            if (parts.Length != 2)
                return false;

            var salt = Convert.FromBase64String(parts[0]);
            var expectedHash = Convert.FromBase64String(parts[1]);

            // Hash the provided password with the same salt
            var actualHash = HashPasswordInternal(password, salt);

            // Constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Internal hashing logic using Argon2id
    /// </summary>
    private byte[] HashPasswordInternal(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };

        return argon2.GetBytes(HashSize);
    }
}
