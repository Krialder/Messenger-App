using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Konscious.Security.Cryptography;

namespace MessengerClient.Services
{
    /// <summary>
    /// Layer 2: Local Storage Encryption Service
    /// Encrypts local data at rest using AES-256-GCM with password-derived master key
    /// Protects against device theft and forensic analysis (DSGVO Art. 32 compliant)
    /// </summary>
    public class LocalCryptoService : IDisposable
    {
        private byte[]? _masterKey;
        private const int SaltSize = 32;     // 256 bits
        private const int KeySize = 32;      // 256 bits for AES-256
        private const int NonceSize = 12;    // 96 bits for GCM
        private const int TagSize = 16;      // 128 bits for authentication tag

        /// <summary>
        /// Derive master key from user password using Argon2id (memory-hard KDF)
        /// Parameters: 64 MB memory, 3 iterations, 4 parallelism (OWASP recommended)
        /// Master key only exists in RAM during active session
        /// </summary>
        public async Task<byte[]> DeriveMasterKeyAsync(string password, byte[] salt)
        {
            using (Argon2id argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = 4;
                argon2.Iterations = 3;
                argon2.MemorySize = 65536; // 64 MB

                byte[] key = await Task.Run(() => argon2.GetBytes(KeySize));
                _masterKey = key;
                return key;
            }
        }

        /// <summary>
        /// Encrypt local data with AES-256-GCM
        /// Format: [12-byte nonce][ciphertext][16-byte tag] (Base64 encoded)
        /// Returns encrypted data ready for storage in SQLite
        /// </summary>
        public async Task<string> EncryptLocalDataAsync(string plaintext, byte[]? masterKey = null)
        {
            byte[] key = masterKey ?? _masterKey ?? throw new InvalidOperationException("Master key not initialized");

            // Generate cryptographically secure random nonce
            byte[] nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);

            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[TagSize];

            using (AesGcm aesGcm = new AesGcm(key, TagSize))
            {
                await Task.Run(() => aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag));
            }

            // Combine: nonce || ciphertext || tag
            byte[] result = new byte[nonce.Length + ciphertext.Length + tag.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
            Buffer.BlockCopy(ciphertext, 0, result, nonce.Length, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, result, nonce.Length + ciphertext.Length, tag.Length);

            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// Decrypt local data with AES-256-GCM
        /// Verifies authentication tag to detect tampering
        /// Throws CryptographicException if data was modified or wrong key
        /// </summary>
        public async Task<string> DecryptLocalDataAsync(string encryptedData, byte[]? masterKey = null)
        {
            byte[] key = masterKey ?? _masterKey ?? throw new InvalidOperationException("Master key not initialized");

            byte[] data = Convert.FromBase64String(encryptedData);

            byte[] nonce = new byte[NonceSize];
            byte[] tag = new byte[TagSize];
            byte[] ciphertext = new byte[data.Length - NonceSize - TagSize];

            Buffer.BlockCopy(data, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(data, NonceSize, ciphertext, 0, ciphertext.Length);
            Buffer.BlockCopy(data, NonceSize + ciphertext.Length, tag, 0, TagSize);

            byte[] plaintext = new byte[ciphertext.Length];

            using (AesGcm aesGcm = new AesGcm(key, TagSize))
            {
                await Task.Run(() => aesGcm.Decrypt(nonce, ciphertext, tag, plaintext));
            }

            return Encoding.UTF8.GetString(plaintext);
        }

        /// <summary>
        /// Generate cryptographically secure random salt (32 bytes)
        /// Salt is unique per user and stored on server (NOT the master key)
        /// </summary>
        public byte[] GenerateSalt()
        {
            byte[] salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt);
            return salt;
        }

        /// <summary>
        /// Securely erase master key from memory on logout
        /// Uses CryptographicOperations.ZeroMemory for secure overwrite
        /// Prevents memory dumps from exposing the key
        /// </summary>
        public void ClearMasterKey()
        {
            if (_masterKey != null)
            {
                CryptographicOperations.ZeroMemory(_masterKey);
                _masterKey = null;
            }
        }

        /// <summary>
        /// IDisposable implementation for automatic cleanup
        /// Called by Dependency Injection container on app shutdown
        /// </summary>
        public void Dispose()
        {
            ClearMasterKey();
        }
    }
}
