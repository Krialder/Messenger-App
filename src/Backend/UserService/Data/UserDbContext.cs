// ========================================
// PSEUDO-CODE - Sprint 7: User Database Context
// Status: 🔶 Entity Framework Struktur
// ========================================

using Microsoft.EntityFrameworkCore;

namespace SecureMessenger.UserService.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }
    
    // PSEUDO: Shares users table with AuthService
    public DbSet<User> Users => Set<User>();
    public DbSet<Contact> Contacts => Set<Contact>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.DeletionScheduledAt);
        });
        
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.ContactUserId }).IsUnique();
        });
    }
}

// ========================================
// ENTITIES
// ========================================

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public byte[] MasterKeySalt { get; set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; set; }
    public bool EmailVerified { get; set; }
    public bool MfaEnabled { get; set; }
    public DateTime? DeletionScheduledAt { get; set; }
}

public class Contact
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ContactUserId { get; set; }
    public DateTime AddedAt { get; set; }
    public bool IsBlocked { get; set; }
}
