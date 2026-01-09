namespace UserService.Data.Entities;

/// <summary>
/// User profile entity (maps to user_profiles table).
/// </summary>
public class UserProfile
{
    /// <summary>
    /// User ID (matches Auth Service user ID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Username (unique).
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email address (unique).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Master key salt for Layer 2 encryption (32 bytes).
    /// </summary>
    public byte[] MasterKeySalt { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Display name (optional).
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Avatar URL (optional).
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Bio/Status text (optional).
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Account creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last profile update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Whether email is verified.
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// Account deletion scheduled timestamp (DSGVO 30-day grace period).
    /// </summary>
    public DateTime? DeleteScheduledAt { get; set; }

    /// <summary>
    /// Whether account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property: Contacts where this user is the owner.
    /// </summary>
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}
