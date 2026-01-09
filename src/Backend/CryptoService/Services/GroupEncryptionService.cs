using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

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

    /// <summary>
    /// Encrypt message for all group members
    /// </summary>
    public async Task<GroupMessageEncryptionResult> EncryptForGroupAsync(
        string plaintext,
        List<MemberPublicKey> memberPublicKeys)
    {
        // 1. Generate ephemeral group key (AES-256)
        byte[] groupKey = GenerateRandomKey(GroupKeySize);

        // 2. Encrypt message content with group key (AES-256-GCM)
        var (ciphertext, nonce, tag) = EncryptWithAesGcm(plaintext, groupKey);

        // 3. Encrypt group key for each member using their public key (X25519 + ChaCha20-Poly1305)
        var encryptedKeys = new List<EncryptedKeyPair>();
        
        foreach (var member in memberPublicKeys)
        {
            // Use X25519 key exchange + ChaCha20-Poly1305 to encrypt the group key
            var encryptedGroupKey = await EncryptGroupKeyForMemberAsync(groupKey, member.PublicKey);
            
            encryptedKeys.Add(new EncryptedKeyPair
            {
                UserId = member.UserId,
                EncryptedGroupKey = encryptedGroupKey
            });
        }

        // 4. Securely erase group key from memory
        CryptographicOperations.ZeroMemory(groupKey);

        return new GroupMessageEncryptionResult
        {
            EncryptedContent = ciphertext,
            Nonce = nonce,
            Tag = tag,
            EncryptedKeys = encryptedKeys
        };
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
        // 1. Decrypt the group key using our private key
        byte[] groupKey = await DecryptGroupKeyAsync(encryptedGroupKey, myPrivateKey);

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

    private async Task<byte[]> EncryptGroupKeyForMemberAsync(byte[] groupKey, byte[] memberPublicKey)
    {
        // TODO: Implement X25519 key exchange + ChaCha20-Poly1305 encryption
        // For now, this is a placeholder. In production, use libsodium-net or similar.
        
        // Pseudo-implementation:
        // 1. Generate ephemeral key pair (X25519)
        // 2. Compute shared secret: ECDH(ephemeral_private, member_public_key)
        // 3. Derive encryption key: HKDF(shared_secret)
        // 4. Encrypt group key: ChaCha20-Poly1305(group_key, derived_key)
        // 5. Return: ephemeral_public_key || encrypted_group_key || tag

        await Task.CompletedTask; // Placeholder for async operation
        
        // TEMPORARY: For demonstration purposes
        // In production, replace with proper X25519 + ChaCha20-Poly1305
        byte[] encrypted = new byte[32]; // Placeholder
        RandomNumberGenerator.Fill(encrypted);
        return encrypted;
    }

    private async Task<byte[]> DecryptGroupKeyAsync(byte[] encryptedGroupKey, byte[] myPrivateKey)
    {
        // TODO: Implement X25519 key exchange + ChaCha20-Poly1305 decryption
        // This is the reverse of EncryptGroupKeyForMemberAsync
        
        // Pseudo-implementation:
        // 1. Extract ephemeral_public_key, encrypted_data, tag from input
        // 2. Compute shared secret: ECDH(my_private_key, ephemeral_public_key)
        // 3. Derive decryption key: HKDF(shared_secret)
        // 4. Decrypt: ChaCha20-Poly1305 Decrypt(encrypted_data, derived_key, tag)
        // 5. Return decrypted group key

        await Task.CompletedTask; // Placeholder
        
        // TEMPORARY: For demonstration
        byte[] groupKey = new byte[32];
        RandomNumberGenerator.Fill(groupKey);
        return groupKey;
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
