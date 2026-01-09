namespace MessengerContracts.DTOs;

/// <summary>
/// Represents an encrypted message using ChaCha20-Poly1305.
/// </summary>
public class EncryptedMessageDto
{
    /// <summary>
    /// 12-byte nonce for ChaCha20-Poly1305.
    /// </summary>
    public byte[] Nonce { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// The encrypted ciphertext.
    /// </summary>
    public byte[] Ciphertext { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// 16-byte authentication tag from Poly1305.
    /// </summary>
    public byte[] Tag { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// 32-byte ephemeral public key for X25519 key exchange.
    /// </summary>
    public byte[] EphemeralPublicKey { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// Represents a cryptographic key pair (X25519).
/// </summary>
public class KeyPair
{
    /// <summary>
    /// 32-byte public key.
    /// </summary>
    public byte[] PublicKey { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// 32-byte private key (must be securely erased after use).
    /// </summary>
    public byte[] PrivateKey { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// Request to rotate a user's cryptographic key.
/// </summary>
public class RotateKeyRequest
{
    /// <summary>
    /// The new public key (Base64 encoded).
    /// </summary>
    public string NewPublicKey { get; set; } = string.Empty;
}

/// <summary>
/// Represents a public key for a user.
/// </summary>
public class PublicKeyDto
{
    /// <summary>
    /// Unique identifier for the key.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User ID owning this key.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Base64-encoded public key.
    /// </summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// Key creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Key expiration timestamp.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
