using Microsoft.EntityFrameworkCore;
using UserService.Data.Entities;

namespace UserService.Data;

/// <summary>
/// Database context for user profiles and contacts.
/// </summary>
public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// User profiles.
    /// </summary>
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;

    /// <summary>
    /// User contacts.
    /// </summary>
    public DbSet<Contact> Contacts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // UserProfile configuration
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("user_profiles");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.Username)
                .HasColumnName("username")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.MasterKeySalt)
                .HasColumnName("master_key_salt")
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(e => e.DisplayName)
                .HasColumnName("display_name")
                .HasMaxLength(100);

            entity.Property(e => e.AvatarUrl)
                .HasColumnName("avatar_url")
                .HasMaxLength(500);

            entity.Property(e => e.Bio)
                .HasColumnName("bio")
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(e => e.EmailVerified)
                .HasColumnName("email_verified")
                .HasDefaultValue(false);

            entity.Property(e => e.DeleteScheduledAt)
                .HasColumnName("delete_scheduled_at");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            // Indexes
            entity.HasIndex(e => e.Username)
                .IsUnique()
                .HasDatabaseName("idx_user_profiles_username");

            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("idx_user_profiles_email");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("idx_user_profiles_is_active");

            entity.HasIndex(e => e.DeleteScheduledAt)
                .HasDatabaseName("idx_user_profiles_delete_scheduled");
        });

        // Contact configuration
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.ToTable("contacts");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.ContactUserId)
                .HasColumnName("contact_user_id")
                .IsRequired();

            entity.Property(e => e.Nickname)
                .HasColumnName("nickname")
                .HasMaxLength(100);

            entity.Property(e => e.IsBlocked)
                .HasColumnName("is_blocked")
                .HasDefaultValue(false);

            entity.Property(e => e.AddedAt)
                .HasColumnName("added_at")
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            // Relationship
            entity.HasOne(e => e.User)
                .WithMany(u => u.Contacts)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("idx_contacts_user_id");

            entity.HasIndex(e => e.ContactUserId)
                .HasDatabaseName("idx_contacts_contact_user_id");

            entity.HasIndex(e => new { e.UserId, e.ContactUserId })
                .IsUnique()
                .HasDatabaseName("idx_contacts_unique_pair");

            // Constraint: Cannot add self as contact
            entity.ToTable(t => t.HasCheckConstraint("chk_no_self_contact", "user_id != contact_user_id"));
        });
    }
}
