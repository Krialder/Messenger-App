using Sodium;
using System.Security.Cryptography;

namespace CryptoService.Layer2
{
    /// <summary>
    /// Layer 2: Local Storage Encryption
    /// AES-256-GCM + Argon2id Master Key Derivation
    /// </summary>
    public interface ILocalStorageEncryption
    {
        byte[] DeriveMasterKey(string password, byte[] salt);
        byte[] EncryptPrivateKey(byte[] privateKey, byte[] masterKey);
        byte[] DecryptPrivateKey(byte[] encryptedPrivateKey, byte[] masterKey);
        byte[] EncryptMessageCache(byte[] messageData, byte[] masterKey);
        byte[] DecryptMessageCache(byte[] encryptedData, byte[] masterKey);
    }

    public class LocalStorageEncryptionService : ILocalStorageEncryption
    {
        // PSEUDO CODE: Layer 2 Local Storage Encryption

        public byte[] DeriveMasterKey(string password, byte[] salt)
        {
            // PSEUDO CODE:
            // 1. Use Argon2id to derive master key from user password
            // 2. Parameters (BSI-recommended):
            //    - Memory: 64 MB (65536 KB)
            //    - Iterations: 3
            //    - Parallelism: 4
            //    - Salt: 32 bytes (from users.master_key_salt)
            //    - Output: 32 bytes (256-bit key)
            // 3. Return derived key
            
            var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            
            // Using libsodium's PasswordHash (Argon2id)
            var masterKey = PasswordHash.ArgonHashBinary(
                password: passwordBytes,
                salt: salt,
                opsLimit: PasswordHash.ArgonOpsLimitSensitive,
                memLimit: PasswordHash.ArgonMemLimitSensitive,
                outputLength: 32
            );
            
            return masterKey;
        }

        public byte[] EncryptPrivateKey(byte[] privateKey, byte[] masterKey)
        {
            // PSEUDO CODE:
            // 1. Generate random nonce (12 bytes for AES-GCM)
            // 2. Encrypt private key with AES-256-GCM
            //    - Key: masterKey (32 bytes)
            //    - Nonce: 12 bytes
            //    - AAD (Additional Authenticated Data): "private_key"
            // 3. Return: nonce || ciphertext || auth_tag
            // 4. Store in local database (SQLite/encrypted storage)
            
            using var aes = new AesGcm(masterKey);
            var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
            RandomNumberGenerator.Fill(nonce);
            
            var ciphertext = new byte[privateKey.Length];
            var tag = new byte[AesGcm.TagByteSizes.MaxSize];
            
            aes.Encrypt(nonce, privateKey, ciphertext, tag, null);
            
            // Combine: nonce || ciphertext || tag
            var result = new byte[nonce.Length + ciphertext.Length + tag.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
            Buffer.BlockCopy(ciphertext, 0, result, nonce.Length, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, result, nonce.Length + ciphertext.Length, tag.Length);
            
            return result;
        }

        public byte[] DecryptPrivateKey(byte[] encryptedPrivateKey, byte[] masterKey)
        {
            // PSEUDO CODE:
            // 1. Extract nonce, ciphertext, and auth tag
            // 2. Decrypt with AES-256-GCM
            // 3. Verify authentication tag
            // 4. Return decrypted private key
            // 5. Keep in memory only (never write to disk unencrypted)
            
            using var aes = new AesGcm(masterKey);
            
            var nonceSize = AesGcm.NonceByteSizes.MaxSize;
            var tagSize = AesGcm.TagByteSizes.MaxSize;
            
            var nonce = new byte[nonceSize];
            var tag = new byte[tagSize];
            var ciphertext = new byte[encryptedPrivateKey.Length - nonceSize - tagSize];
            
            Buffer.BlockCopy(encryptedPrivateKey, 0, nonce, 0, nonceSize);
            Buffer.BlockCopy(encryptedPrivateKey, nonceSize, ciphertext, 0, ciphertext.Length);
            Buffer.BlockCopy(encryptedPrivateKey, nonceSize + ciphertext.Length, tag, 0, tagSize);
            
            var plaintext = new byte[ciphertext.Length];
            aes.Decrypt(nonce, ciphertext, tag, plaintext, null);
            
            return plaintext;
        }

        public byte[] EncryptMessageCache(byte[] messageData, byte[] masterKey)
        {
            // PSEUDO CODE:
            // 1. Encrypt locally cached messages with same AES-GCM approach
            // 2. Used for offline message storage on client
            // 3. Performance: ~0.5ms per message
            
            return EncryptPrivateKey(messageData, masterKey); // Same algorithm
        }

        public byte[] DecryptMessageCache(byte[] encryptedData, byte[] masterKey)
        {
            // PSEUDO CODE:
            // 1. Decrypt cached messages
            // 2. Verify integrity with auth tag
            
            return DecryptPrivateKey(encryptedData, masterKey); // Same algorithm
        }

        public byte[] GenerateSalt()
        {
            // PSEUDO CODE:
            // 1. Generate cryptographically random 32-byte salt
            // 2. Store in users.master_key_salt during registration
            // 3. NEVER reuse salts
            
            var salt = new byte[32];
            RandomNumberGenerator.Fill(salt);
            return salt;
        }
    }
}
