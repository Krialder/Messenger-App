namespace UserService.Data.Entities;

/// <summary>
/// Contact relationship entity (maps to contacts table).
/// </summary>
public class Contact
{
    /// <summary>
    /// Contact ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User ID who owns this contact.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Contact user ID (the user being added as contact).
    /// </summary>
    public Guid ContactUserId { get; set; }

    /// <summary>
    /// Custom nickname for this contact (optional).
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// Whether this contact is blocked.
    /// </summary>
    public bool IsBlocked { get; set; }

    /// <summary>
    /// When the contact was added.
    /// </summary>
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Navigation property: Owner user profile.
    /// </summary>
    public UserProfile? User { get; set; }
}
