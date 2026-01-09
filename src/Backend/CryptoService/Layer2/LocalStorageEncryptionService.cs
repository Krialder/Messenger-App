using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using MessengerContracts.Interfaces;

namespace CryptoService.Layer2;

/// <summary>
/// Implementation of Layer 2 local storage encryption using AES-256-GCM and Argon2id.
/// Protects data at rest on the client device.
/// </summary>
public class LocalStorageEncryptionService : ILocalStorageEncryptionService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int KeySize = 32;
    private const int SaltSize = 32;

    // Argon2id parameters (OWASP recommended for password hashing)
    private const int Argon2MemorySize = 65536; // 64 MB
    private const int Argon2Iterations = 3;
    private const int Argon2Parallelism = 4;

    /// <summary>
    /// Derives a master key from a password and user salt using Argon2id.
    /// Performance: ~100-200ms (intentionally slow for security).
    /// </summary>
    public async Task<byte[]> DeriveMasterKeyAsync(string password, byte[] userSalt)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }

        if (userSalt == null || userSalt.Length != SaltSize)
        {
            throw new ArgumentException($"User salt must be {SaltSize} bytes.", nameof(userSalt));
        }

        return await Task.Run(() =>
        {
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = userSalt,
                DegreeOfParallelism = Argon2Parallelism,
                MemorySize = Argon2MemorySize,
                Iterations = Argon2Iterations
            };

            return argon2.GetBytes(KeySize);
        });
    }

    /// <summary>
    /// Encrypts plaintext for local storage using AES-256-GCM.
    /// Performance target: &lt; 0.5ms.
    /// </summary>
    public async Task<string> EncryptLocalDataAsync(string plaintext, byte[] masterKey)
    {
        if (string.IsNullOrEmpty(plaintext))
        {
            throw new ArgumentException("Plaintext cannot be null or empty.", nameof(plaintext));
        }

        if (masterKey == null || masterKey.Length != KeySize)
        {
            throw new ArgumentException($"Master key must be {KeySize} bytes.", nameof(masterKey));
        }

        return await Task.Run(() =>
        {
            byte[] nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);

            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[TagSize];

            using var aesGcm = new AesGcm(masterKey, TagSize);
            aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);

            // Combine: nonce || ciphertext || tag
            byte[] combined = new byte[NonceSize + ciphertext.Length + TagSize];
            Array.Copy(nonce, 0, combined, 0, NonceSize);
            Array.Copy(ciphertext, 0, combined, NonceSize, ciphertext.Length);
            Array.Copy(tag, 0, combined, NonceSize + ciphertext.Length, TagSize);

            return Convert.ToBase64String(combined);
        });
    }

    /// <summary>
    /// Decrypts locally stored data using AES-256-GCM.
    /// Performance target: &lt; 0.5ms.
    /// </summary>
    public async Task<string> DecryptLocalDataAsync(string encryptedData, byte[] masterKey)
    {
        if (string.IsNullOrEmpty(encryptedData))
        {
            throw new ArgumentException("Encrypted data cannot be null or empty.", nameof(encryptedData));
        }

        if (masterKey == null || masterKey.Length != KeySize)
        {
            throw new ArgumentException($"Master key must be {KeySize} bytes.", nameof(masterKey));
        }

        return await Task.Run(() =>
        {
            byte[] combined = Convert.FromBase64String(encryptedData);

            if (combined.Length < NonceSize + TagSize)
            {
                throw new ArgumentException("Invalid encrypted data format.", nameof(encryptedData));
            }

            // Extract: nonce || ciphertext || tag
            byte[] nonce = new byte[NonceSize];
            byte[] tag = new byte[TagSize];
            byte[] ciphertext = new byte[combined.Length - NonceSize - TagSize];

            Array.Copy(combined, 0, nonce, 0, NonceSize);
            Array.Copy(combined, NonceSize, ciphertext, 0, ciphertext.Length);
            Array.Copy(combined, NonceSize + ciphertext.Length, tag, 0, TagSize);

            byte[] plaintext = new byte[ciphertext.Length];

            try
            {
                using var aesGcm = new AesGcm(masterKey, TagSize);
                aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);

                return Encoding.UTF8.GetString(plaintext);
            }
            catch (CryptographicException)
            {
                throw new CryptographicException("Decryption failed. Invalid ciphertext, key, or authentication tag.");
            }
        });
    }

    /// <summary>
    /// Stores a private key encrypted with the master key.
    /// In a real implementation, this would write to a local database or file.
    /// </summary>
    public async Task StorePrivateKeyAsync(byte[] privateKey, byte[] masterKey)
    {
        if (privateKey == null || privateKey.Length == 0)
        {
            throw new ArgumentException("Private key cannot be null or empty.", nameof(privateKey));
        }

        if (masterKey == null || masterKey.Length != KeySize)
        {
            throw new ArgumentException($"Master key must be {KeySize} bytes.", nameof(masterKey));
        }

        // Convert private key to Base64 for encryption
        string privateKeyBase64 = Convert.ToBase64String(privateKey);
        string encryptedPrivateKey = await EncryptLocalDataAsync(privateKeyBase64, masterKey);

        // TODO: Store encryptedPrivateKey in local storage (SQLite, file, etc.)
        // For now, this is a placeholder
        await Task.CompletedTask;
    }

    /// <summary>
    /// Loads and decrypts a private key using the master key.
    /// In a real implementation, this would read from a local database or file.
    /// </summary>
    public async Task<byte[]> LoadPrivateKeyAsync(byte[] masterKey)
    {
        if (masterKey == null || masterKey.Length != KeySize)
        {
            throw new ArgumentException($"Master key must be {KeySize} bytes.", nameof(masterKey));
        }

        // TODO: Load encryptedPrivateKey from local storage
        // For now, this is a placeholder that returns an empty array
        string encryptedPrivateKey = string.Empty; // Replace with actual load

        if (string.IsNullOrEmpty(encryptedPrivateKey))
        {
            throw new InvalidOperationException("No private key found in local storage.");
        }

        string privateKeyBase64 = await DecryptLocalDataAsync(encryptedPrivateKey, masterKey);
        return Convert.FromBase64String(privateKeyBase64);
    }
}
