using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Data.Entities;

[Table("mfa_methods")]
public class MfaMethod
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("method_type")]
    [StringLength(20)]
    public string MethodType { get; set; } = string.Empty; // "totp", "yubikey", "fido2"

    [Column("totp_secret_encrypted")]
    [StringLength(500)]
    public string? TotpSecretEncrypted { get; set; }

    [Column("yubikey_public_id")]
    [StringLength(100)]
    public string? YubikeyPublicId { get; set; }

    [Column("fido2_credential_id")]
    [StringLength(500)]
    public string? Fido2CredentialId { get; set; }

    [Column("fido2_public_key")]
    [StringLength(1000)]
    public string? Fido2PublicKey { get; set; }

    [Column("is_primary")]
    public bool IsPrimary { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("friendly_name")]
    [StringLength(100)]
    public string? FriendlyName { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("last_used_at")]
    public DateTime? LastUsedAt { get; set; }

    // Navigation property
    public User User { get; set; } = null!;

    // TOTP Secret encryption/decryption methods
    [NotMapped]
    public string? TotpSecret
    {
        get
        {
            if (string.IsNullOrEmpty(TotpSecretEncrypted))
                return null;
            
            return DecryptTotpSecret(TotpSecretEncrypted);
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                TotpSecretEncrypted = null;
            }
            else
            {
                TotpSecretEncrypted = EncryptTotpSecret(value);
            }
        }
    }

    private static string EncryptTotpSecret(string secret)
    {
        try
        {
            // Get encryption key from configuration or environment
            var encryptionKey = GetApplicationEncryptionKey();
            
            using var aes = Aes.Create();
            aes.Key = encryptionKey;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var encryptedBytes = encryptor.TransformFinalBlock(secretBytes, 0, secretBytes.Length);

            // Combine IV + encrypted data
            var result = new byte[aes.IV.Length + encryptedBytes.Length];
            Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
            Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

            return Convert.ToBase64String(result);
        }
        catch (Exception)
        {
            throw new InvalidOperationException("Failed to encrypt TOTP secret");
        }
    }

    private static string DecryptTotpSecret(string encryptedSecret)
    {
        try
        {
            var encryptionKey = GetApplicationEncryptionKey();
            var encryptedData = Convert.FromBase64String(encryptedSecret);

            using var aes = Aes.Create();
            aes.Key = encryptionKey;

            // Extract IV (first 16 bytes)
            var iv = new byte[16];
            var ciphertext = new byte[encryptedData.Length - 16];
            Array.Copy(encryptedData, 0, iv, 0, 16);
            Array.Copy(encryptedData, 16, ciphertext, 0, ciphertext.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception)
        {
            throw new InvalidOperationException("Failed to decrypt TOTP secret");
        }
    }

    private static byte[] GetApplicationEncryptionKey()
    {
        // In production, this should come from Azure Key Vault, AWS KMS, or similar
        var keyString = Environment.GetEnvironmentVariable("TOTP_ENCRYPTION_KEY")
            ?? "dev_totp_encryption_key_32_chars_min";  // Development fallback

        if (keyString.Length < 32)
        {
            throw new InvalidOperationException(
                "TOTP encryption key must be at least 32 characters. " +
                "Set TOTP_ENCRYPTION_KEY environment variable.");
        }

        // Use SHA256 to derive exactly 32 bytes from the key string
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(Encoding.UTF8.GetBytes(keyString));
    }
}
