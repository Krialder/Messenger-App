using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Sodium;

namespace CryptoService.Services;

/// <summary>
/// Service for encrypting messages in group conversations
/// Implements Signal Protocol approach: Encrypt message with group key, encrypt group key for each member
/// </summary>
public interface IGroupEncryptionService
{
    /// <summary>
    /// Encrypt a message for all members of a group
    /// </summary>
    Task<GroupMessageEncryptionResult> EncryptForGroupAsync(
        string plaintext,
        List<MemberPublicKey> memberPublicKeys);

    /// <summary>
    /// Decrypt a group message
    /// </summary>
    Task<string> DecryptGroupMessageAsync(
        byte[] encryptedContent,
        byte[] nonce,
        byte[] tag,
        byte[] encryptedGroupKey,
        byte[] myPrivateKey);

    /// <summary>
    /// Rotate group key when membership changes
    /// </summary>
    Task<byte[]> GenerateNewGroupKeyAsync();
}

public class GroupEncryptionService : IGroupEncryptionService
{
    private const int GroupKeySize = 32; // AES-256
    private const int NonceSize = 12;   // AES-GCM nonce
    private const int TagSize = 16;     // AES-GCM authentication tag
    private const int PublicKeySize = 32; // X25519 public key
    private const int PrivateKeySize = 32; // X25519 private key
    private const int SecretBoxNonceSize = 24; // SecretBox (ChaCha20-Poly1305) nonce

    /// <summary>
    /// Encrypt message for all group members
    /// </summary>
    public async Task<GroupMessageEncryptionResult> EncryptForGroupAsync(
        string plaintext,
        List<MemberPublicKey> memberPublicKeys)
    {
        return await Task.Run(() =>
        {
            // 1. Generate ephemeral group key (AES-256)
            byte[] groupKey = GenerateRandomKey(GroupKeySize);

            try
            {
                // 2. Encrypt message content with group key (AES-256-GCM)
                var (ciphertext, nonce, tag) = EncryptWithAesGcm(plaintext, groupKey);

                // 3. Encrypt group key for each member using their public key (X25519 + ChaCha20-Poly1305)
                var encryptedKeys = new List<EncryptedKeyPair>();

                foreach (var member in memberPublicKeys)
                {
                    // Use X25519 key exchange + ChaCha20-Poly1305 to encrypt the group key
                    var encryptedGroupKey = EncryptGroupKeyForMember(groupKey, member.PublicKey);

                    encryptedKeys.Add(new EncryptedKeyPair
                    {
                        UserId = member.UserId,
                        EncryptedGroupKey = encryptedGroupKey
                    });
                }

                return new GroupMessageEncryptionResult
                {
                    EncryptedContent = ciphertext,
                    Nonce = nonce,
                    Tag = tag,
                    EncryptedKeys = encryptedKeys
                };
            }
            finally
            {
                // 4. Securely erase group key from memory
                CryptographicOperations.ZeroMemory(groupKey);
            }
        });
    }

    /// <summary>
    /// Decrypt a group message using the encrypted group key
    /// </summary>
    public async Task<string> DecryptGroupMessageAsync(
        byte[] encryptedContent,
        byte[] nonce,
        byte[] tag,
        byte[] encryptedGroupKey,
        byte[] myPrivateKey)
    {
        return await Task.Run(() =>
        {
            // 1. Decrypt the group key using our private key
            byte[] groupKey = DecryptGroupKey(encryptedGroupKey, myPrivateKey);

            try
            {
                // 2. Decrypt the message content using the group key
                string plaintext = DecryptWithAesGcm(encryptedContent, nonce, tag, groupKey);
                return plaintext;
            }
            finally
            {
                // 3. Securely erase group key from memory
                CryptographicOperations.ZeroMemory(groupKey);
            }
        });
    }

    /// <summary>
    /// Generate a new random group key for key rotation
    /// </summary>
    public Task<byte[]> GenerateNewGroupKeyAsync()
    {
        byte[] groupKey = GenerateRandomKey(GroupKeySize);
        return Task.FromResult(groupKey);
    }

    // ========================================
    // Private Helper Methods
    // ========================================

    private byte[] GenerateRandomKey(int size)
    {
        byte[] key = new byte[size];
        RandomNumberGenerator.Fill(key);
        return key;
    }

    private (byte[] ciphertext, byte[] nonce, byte[] tag) EncryptWithAesGcm(string plaintext, byte[] key)
    {
        byte[] nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        byte[] tag = new byte[TagSize];
        byte[] plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
        byte[] ciphertext = new byte[plaintextBytes.Length];

        using var aesGcm = new AesGcm(key, TagSize);
        aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        return (ciphertext, nonce, tag);
    }

    private string DecryptWithAesGcm(byte[] ciphertext, byte[] nonce, byte[] tag, byte[] key)
    {
        byte[] plaintext = new byte[ciphertext.Length];

        using var aesGcm = new AesGcm(key, TagSize);
        aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);

        return System.Text.Encoding.UTF8.GetString(plaintext);
    }

    /// <summary>
    /// Encrypt group key for a specific member using X25519 + ChaCha20-Poly1305
    /// </summary>
    private byte[] EncryptGroupKeyForMember(byte[] groupKey, byte[] memberPublicKey)
    {
        if (memberPublicKey == null || memberPublicKey.Length != PublicKeySize)
        {
            throw new ArgumentException($"Member public key must be {PublicKeySize} bytes.", nameof(memberPublicKey));
        }

        // 1. Generate ephemeral key pair (X25519)
        var ephemeralKeyPair = PublicKeyBox.GenerateKeyPair();
        byte[] ephemeralPublicKey = ephemeralKeyPair.PublicKey;
        byte[] ephemeralPrivateKey = ephemeralKeyPair.PrivateKey;

        byte[] sharedSecret = null!;

        try
        {
            // 2. Compute shared secret: ECDH(ephemeral_private, member_public_key)
            sharedSecret = ScalarMult.Mult(ephemeralPrivateKey, memberPublicKey);

            // 3. Generate random nonce for SecretBox (ChaCha20-Poly1305)
            byte[] nonce = new byte[SecretBoxNonceSize];
            RandomNumberGenerator.Fill(nonce);

            // 4. Encrypt group key using ChaCha20-Poly1305 (SecretBox)
            byte[] encryptedGroupKey = SecretBox.Create(groupKey, nonce, sharedSecret);

            // 5. Return: ephemeral_public_key || nonce || encrypted_group_key
            byte[] result = new byte[PublicKeySize + SecretBoxNonceSize + encryptedGroupKey.Length];
            Array.Copy(ephemeralPublicKey, 0, result, 0, PublicKeySize);
            Array.Copy(nonce, 0, result, PublicKeySize, SecretBoxNonceSize);
            Array.Copy(encryptedGroupKey, 0, result, PublicKeySize + SecretBoxNonceSize, encryptedGroupKey.Length);

            return result;
        }
        finally
        {
            // Securely erase ephemeral private key and shared secret
            if (ephemeralPrivateKey != null)
            {
                CryptographicOperations.ZeroMemory(ephemeralPrivateKey);
            }

            if (sharedSecret != null)
            {
                CryptographicOperations.ZeroMemory(sharedSecret);
            }
        }
    }

    /// <summary>
    /// Decrypt group key using our private key
    /// </summary>
    private byte[] DecryptGroupKey(byte[] encryptedData, byte[] myPrivateKey)
    {
        if (myPrivateKey == null || myPrivateKey.Length != PrivateKeySize)
        {
            throw new ArgumentException($"Private key must be {PrivateKeySize} bytes.", nameof(myPrivateKey));
        }

        if (encryptedData == null || encryptedData.Length < PublicKeySize + SecretBoxNonceSize + TagSize)
        {
            throw new ArgumentException("Invalid encrypted group key format.", nameof(encryptedData));
        }

        // 1. Extract ephemeral_public_key, nonce, encrypted_data from input
        byte[] ephemeralPublicKey = new byte[PublicKeySize];
        byte[] nonce = new byte[SecretBoxNonceSize];
        byte[] encryptedGroupKey = new byte[encryptedData.Length - PublicKeySize - SecretBoxNonceSize];

        Array.Copy(encryptedData, 0, ephemeralPublicKey, 0, PublicKeySize);
        Array.Copy(encryptedData, PublicKeySize, nonce, 0, SecretBoxNonceSize);
        Array.Copy(encryptedData, PublicKeySize + SecretBoxNonceSize, encryptedGroupKey, 0, encryptedGroupKey.Length);

        byte[] sharedSecret = null!;

        try
        {
            // 2. Compute shared secret: ECDH(my_private_key, ephemeral_public_key)
            sharedSecret = ScalarMult.Mult(myPrivateKey, ephemeralPublicKey);

            // 3. Decrypt using ChaCha20-Poly1305 (SecretBox)
            byte[] groupKey = SecretBox.Open(encryptedGroupKey, nonce, sharedSecret);

            return groupKey;
        }
        catch (Exception ex)
        {
            throw new CryptographicException("Failed to decrypt group key. Invalid ciphertext or key.", ex);
        }
        finally
        {
            // Securely erase shared secret
            if (sharedSecret != null)
            {
                CryptographicOperations.ZeroMemory(sharedSecret);
            }
        }
    }
}

// ========================================
// Supporting DTOs
// ========================================

/// <summary>
/// Result of encrypting a message for a group
/// </summary>
public class GroupMessageEncryptionResult
{
    public byte[] EncryptedContent { get; set; } = Array.Empty<byte>();
    public byte[] Nonce { get; set; } = Array.Empty<byte>();
    public byte[] Tag { get; set; } = Array.Empty<byte>();
    public List<EncryptedKeyPair> EncryptedKeys { get; set; } = new();
}

/// <summary>
/// Encrypted group key for a specific user
/// </summary>
public class EncryptedKeyPair
{
    public Guid UserId { get; set; }
    public byte[] EncryptedGroupKey { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// Member's public key information
/// </summary>
public class MemberPublicKey
{
    public Guid UserId { get; set; }
    public byte[] PublicKey { get; set; } = Array.Empty<byte>();
}
