// ========================================
// PSEUDO-CODE - Sprint 7: User Service
// Status: 🔶 API-Struktur definiert
// Dependencies: Sprint 2 (AuthService)
// ========================================

using MessengerContracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserService.Data;
using UserService.Data.Entities;
using FluentValidation;

namespace UserService.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserDbContext _context;
    private readonly ILogger<UsersController> _logger;
    private readonly IValidator<UpdateProfileRequest> _updateProfileValidator;
    private readonly IValidator<AddContactRequest> _addContactValidator;
    private readonly IValidator<DeleteAccountRequest> _deleteAccountValidator;

    public UsersController(
        UserDbContext context, 
        ILogger<UsersController> logger,
        IValidator<UpdateProfileRequest> updateProfileValidator,
        IValidator<AddContactRequest> addContactValidator,
        IValidator<DeleteAccountRequest> deleteAccountValidator)
    {
        _context = context;
        _logger = logger;
        _updateProfileValidator = updateProfileValidator;
        _addContactValidator = addContactValidator;
        _deleteAccountValidator = deleteAccountValidator;
    }

    /// <summary>
    /// Get user profile by ID
    /// </summary>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(Guid userId)
    {
        try
        {
            var profile = await _context.UserProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

            if (profile == null)
            {
                _logger.LogWarning("User profile {UserId} not found", userId);
                return NotFound(new { message = "User not found" });
            }

            var userDto = MapToUserDto(profile);
            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving the profile" });
        }
    }

    /// <summary>
    /// Get current authenticated user's profile
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyProfile()
    {
        return await GetProfile(GetUserId());
    }

    /// <summary>
    /// Update current user's profile
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var validationResult = await _updateProfileValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var userId = GetUserId();
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == userId);

            if (profile == null)
            {
                _logger.LogWarning("User profile {UserId} not found for update", userId);
                return NotFound(new { message = "User not found" });
            }

            // Update fields
            if (!string.IsNullOrWhiteSpace(request.DisplayName))
            {
                profile.DisplayName = SanitizeInput(request.DisplayName);
            }

            if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
            {
                profile.AvatarUrl = request.AvatarUrl.Trim();
            }

            if (request.Bio != null)
            {
                profile.Bio = SanitizeInput(request.Bio);
            }

            profile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User profile {UserId} updated successfully", userId);

            var userDto = MapToUserDto(profile);
            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, new { message = "An error occurred while updating the profile" });
        }
    }

    /// <summary>
    /// Get master key salt for a user (for client-side encryption)
    /// </summary>
    [HttpGet("{userId}/salt")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSalt(Guid userId)
    {
        try
        {
            var profile = await _context.UserProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

            if (profile == null)
            {
                _logger.LogWarning("Salt requested for non-existent user {UserId}", userId);
                return NotFound(new { message = "User not found" });
            }

            return Ok(new
            {
                userId = profile.Id,
                salt = Convert.ToBase64String(profile.MasterKeySalt)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving salt for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Search for users by username, email, or display name
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<UserSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] string q, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return BadRequest(new { message = "Search query must be at least 2 characters" });
            }

            if (pageSize > 50) pageSize = 50;
            if (pageSize < 1) pageSize = 10;
            if (page < 1) page = 1;

            var userId = GetUserId();
            var searchTerm = SanitizeInput(q).ToLower();

            var skip = (page - 1) * pageSize;

            var profiles = await _context.UserProfiles
                .AsNoTracking()
                .Where(u => u.IsActive 
                    && u.Id != userId 
                    && (u.Username.ToLower().Contains(searchTerm) 
                        || u.Email.ToLower().Contains(searchTerm) 
                        || (u.DisplayName != null && u.DisplayName.ToLower().Contains(searchTerm))))
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var contactUserIds = await _context.Contacts
                .Where(c => c.UserId == userId)
                .Select(c => c.ContactUserId)
                .ToListAsync();

            var results = profiles.Select(p => new UserSearchResultDto
            {
                Id = p.Id,
                Username = p.Username,
                DisplayName = p.DisplayName,
                AvatarUrl = p.AvatarUrl,
                Bio = p.Bio,
                IsContact = contactUserIds.Contains(p.Id)
            });

            _logger.LogInformation("User search for '{Query}' returned {Count} results", q, results.Count());

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with query '{Query}'", q);
            return StatusCode(500, new { message = "An error occurred during search" });
        }
    }

    /// <summary>
    /// Add a contact
    /// </summary>
    [HttpPost("contacts")]
    [ProducesResponseType(typeof(Contact), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddContact([FromBody] AddContactRequest request)
    {
        try
        {
            var validationResult = await _addContactValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var userId = GetUserId();

            if (request.ContactUserId == userId)
            {
                return BadRequest(new { message = "Cannot add yourself as a contact" });
            }

            // Check if contact user exists
            var contactUserExists = await _context.UserProfiles
                .AnyAsync(u => u.Id == request.ContactUserId && u.IsActive);

            if (!contactUserExists)
            {
                return BadRequest(new { message = "Contact user does not exist" });
            }

            // Check if already a contact
            var existingContact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ContactUserId == request.ContactUserId);

            if (existingContact != null)
            {
                return Conflict(new { message = "User is already a contact" });
            }

            var contact = new Contact
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ContactUserId = request.ContactUserId,
                Nickname = !string.IsNullOrWhiteSpace(request.Nickname) 
                    ? SanitizeInput(request.Nickname) 
                    : null,
                AddedAt = DateTime.UtcNow
            };

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} added contact {ContactUserId}", userId, request.ContactUserId);

            return Ok(contact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding contact");
            return StatusCode(500, new { message = "An error occurred while adding contact" });
        }
    }

    /// <summary>
    /// Get all contacts for current user
    /// </summary>
    [HttpGet("contacts")]
    [ProducesResponseType(typeof(IEnumerable<ContactDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContacts()
    {
        try
        {
            var userId = GetUserId();

            var contacts = await _context.Contacts
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var contactUserIds = contacts.Select(c => c.ContactUserId).ToList();

            var profiles = await _context.UserProfiles
                .Where(u => contactUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var contactDtos = contacts
                .Where(c => profiles.ContainsKey(c.ContactUserId))
                .Select(c =>
                {
                    var profile = profiles[c.ContactUserId];
                    return new ContactDto
                    {
                        Id = c.Id,
                        ContactUserId = c.ContactUserId,
                        Username = profile.Username,
                        DisplayName = profile.DisplayName,
                        Nickname = c.Nickname,
                        AvatarUrl = profile.AvatarUrl,
                        IsBlocked = c.IsBlocked,
                        AddedAt = c.AddedAt
                    };
                })
                .ToList();

            return Ok(contactDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contacts");
            return StatusCode(500, new { message = "An error occurred while retrieving contacts" });
        }
    }

    /// <summary>
    /// Remove a contact
    /// </summary>
    [HttpDelete("contacts/{contactId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveContact(Guid contactId)
    {
        try
        {
            var userId = GetUserId();

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);

            if (contact == null)
            {
                return NotFound(new { message = "Contact not found" });
            }

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} removed contact {ContactId}", userId, contactId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing contact {ContactId}", contactId);
            return StatusCode(500, new { message = "An error occurred while removing contact" });
        }
    }

    /// <summary>
    /// Schedule account deletion (30-day grace period)
    /// </summary>
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
    {
        try
        {
            var validationResult = await _deleteAccountValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var userId = GetUserId();

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (profile == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Schedule deletion for 30 days from now
            profile.DeleteScheduledAt = DateTime.UtcNow.AddDays(30);
            profile.IsActive = false; // Immediately deactivate account
            profile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogWarning("User {UserId} scheduled account deletion for {DeletionDate}", 
                userId, profile.DeleteScheduledAt);

            return Ok(new
            {
                message = "Account deletion scheduled. Your account will be deleted in 30 days.",
                scheduledDate = profile.DeleteScheduledAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling account deletion");
            return StatusCode(500, new { message = "An error occurred while scheduling account deletion" });
        }
    }

    #region Helper Methods

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            _logger.LogError("User ID claim not found in token");
            throw new UnauthorizedAccessException("User ID not found");
        }
        return Guid.Parse(userIdClaim);
    }

    private static UserDto MapToUserDto(UserProfile profile)
    {
        return new UserDto
        {
            Id = profile.Id,
            Username = profile.Username,
            Email = profile.Email,
            DisplayName = profile.DisplayName,
            AvatarUrl = profile.AvatarUrl,
            Bio = profile.Bio,
            MfaEnabled = false,
            EmailVerified = profile.EmailVerified,
            CreatedAt = profile.CreatedAt
        };
    }

    private static string SanitizeInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return input
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("&", "&amp;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#x27;")
            .Trim();
    }

    #endregion
}
