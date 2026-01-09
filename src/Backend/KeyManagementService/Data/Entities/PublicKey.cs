namespace KeyManagementService.Data.Entities;

/// <summary>
/// Represents a user's public key for end-to-end encryption.
/// Keys are automatically rotated after 1 year.
/// </summary>
public class PublicKey
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
    /// 32-byte X25519 public key.
    /// </summary>
    public byte[] Key { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Algorithm used (X25519).
    /// </summary>
    public string Algorithm { get; set; } = "X25519";

    /// <summary>
    /// Key creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Key expiration timestamp (1 year after creation).
    /// </summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddYears(1);

    /// <summary>
    /// Whether this key is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when key was revoked (if applicable).
    /// </summary>
    public DateTime? RevokedAt { get; set; }
}
