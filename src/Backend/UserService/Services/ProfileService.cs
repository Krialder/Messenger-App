// ========================================
// PSEUDO-CODE - Sprint 7: User Profile Service
// Status: 🔶 Interface definiert
// ========================================

namespace SecureMessenger.UserService.Services;

/// <summary>
/// Service for user profile management
/// </summary>
public interface IProfileService
{
    // INTERFACE: Get user profile by ID
    Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // INTERFACE: Update user profile
    Task UpdateProfileAsync(Guid userId, UpdateProfileDto dto, CancellationToken cancellationToken = default);
    
    // INTERFACE: Delete account (schedule for deletion)
    Task ScheduleAccountDeletionAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // INTERFACE: Cancel account deletion
    Task CancelAccountDeletionAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // INTERFACE: Export user data (DSGVO)
    Task<Guid> StartDataExportAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Background service for account deletion (30-day grace period)
/// </summary>
public class AccountDeletionService
{
    // PSEUDO: Runs daily
    // PSEUDO: Find accounts where deletion_scheduled_at < NOW()
    // PSEUDO: Execute deletion:
    //   1. Delete all messages (sender/recipient)
    //   2. Delete all contacts
    //   3. Delete all keys
    //   4. Delete user profile
    //   5. Overwrite sensitive data 3x (DoD 5220.22-M)
    // PSEUDO: Log deletion (Audit)
    
    public async Task ExecutePendingDeletionsAsync(CancellationToken cancellationToken)
    {
        // PSEUDO: Database query for pending deletions
        // var pendingDeletions = await _dbContext.Users
        //     .Where(u => u.DeletionScheduledAt.HasValue && u.DeletionScheduledAt < DateTime.UtcNow)
        //     .ToListAsync(cancellationToken);
        
        // PSEUDO: For each user → execute deletion
    }
}

// ========================================
// DTOs
// ========================================

public record UserProfileDto(
    Guid Id,
    string Username,
    string Email,
    DateTime CreatedAt,
    bool MfaEnabled,
    bool EmailVerified,
    DateTime? DeletionScheduledAt
);

public record UpdateProfileDto(
    string? Email,
    string? DisplayName
);
