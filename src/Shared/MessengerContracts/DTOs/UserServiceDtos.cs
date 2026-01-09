namespace MessengerContracts.DTOs;

/// <summary>
/// Request to update user profile.
/// </summary>
public class UpdateProfileRequest
{
    /// <summary>
    /// Display name (optional).
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Avatar URL (optional).
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Bio/Status text (optional, max 500 chars).
    /// </summary>
    public string? Bio { get; set; }
}

/// <summary>
/// Request to add a contact.
/// </summary>
public class AddContactRequest
{
    /// <summary>
    /// Contact user ID to add.
    /// </summary>
    public Guid ContactUserId { get; set; }

    /// <summary>
    /// Custom nickname for this contact (optional).
    /// </summary>
    public string? Nickname { get; set; }
}

/// <summary>
/// Request to update contact details.
/// </summary>
public class UpdateContactRequest
{
    /// <summary>
    /// Custom nickname for this contact (optional).
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// Whether to block this contact.
    /// </summary>
    public bool IsBlocked { get; set; }
}

/// <summary>
/// Contact DTO.
/// </summary>
public class ContactDto
{
    /// <summary>
    /// Contact ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Contact user ID.
    /// </summary>
    public Guid ContactUserId { get; set; }

    /// <summary>
    /// Contact username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Contact display name (if set).
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Custom nickname for this contact.
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// Contact avatar URL.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Whether this contact is blocked.
    /// </summary>
    public bool IsBlocked { get; set; }

    /// <summary>
    /// When the contact was added.
    /// </summary>
    public DateTime AddedAt { get; set; }
}

/// <summary>
/// Request to delete account (DSGVO).
/// </summary>
public class DeleteAccountRequest
{
    /// <summary>
    /// User password confirmation.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Reason for deletion (optional, for analytics).
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// User search result DTO.
/// </summary>
public class UserSearchResultDto
{
    /// <summary>
    /// User ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Avatar URL.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Bio.
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Whether user is already a contact.
    /// </summary>
    public bool IsContact { get; set; }
}
