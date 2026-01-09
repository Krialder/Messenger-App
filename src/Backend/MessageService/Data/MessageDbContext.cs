using Microsoft.EntityFrameworkCore;
using MessageService.Data.Entities;

namespace MessageService.Data;

/// <summary>
/// Entity Framework DbContext for Message Service
/// </summary>
public class MessageDbContext : DbContext
{
    public MessageDbContext(DbContextOptions<MessageDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Conversation> Conversations { get; set; } = null!;
    public DbSet<ConversationMember> ConversationMembers { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================================
        // Conversation Configuration
        // ========================================

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.ToTable("conversations");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Type)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(c => c.Name)
                .HasMaxLength(100);

            entity.Property(c => c.Description)
                .HasMaxLength(500);

            entity.Property(c => c.AvatarUrl)
                .HasMaxLength(255);

            entity.HasIndex(c => c.CreatedBy);
            entity.HasIndex(c => c.CreatedAt);
            entity.HasIndex(c => new { c.Type, c.IsActive });
        });

        // ========================================
        // ConversationMember Configuration
        // ========================================

        modelBuilder.Entity<ConversationMember>(entity =>
        {
            entity.ToTable("conversation_members");
            entity.HasKey(cm => cm.Id);

            entity.Property(cm => cm.Role)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(cm => cm.CustomNickname)
                .HasMaxLength(100);

            // Foreign Key
            entity.HasOne(cm => cm.Conversation)
                .WithMany(c => c.Members)
                .HasForeignKey(cm => cm.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(cm => cm.ConversationId);
            entity.HasIndex(cm => cm.UserId);
            entity.HasIndex(cm => new { cm.ConversationId, cm.UserId });
            entity.HasIndex(cm => new { cm.UserId, cm.LeftAt })
                .HasFilter("left_at IS NULL"); // Active memberships only
        });

        // ========================================
        // Message Configuration
        // ========================================

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("messages");
            entity.HasKey(m => m.Id);

            entity.Property(m => m.EncryptedContent)
                .IsRequired();

            entity.Property(m => m.Nonce)
                .IsRequired();

            entity.Property(m => m.Status)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(m => m.Type)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(m => m.EncryptedGroupKeys)
                .HasColumnType("jsonb"); // PostgreSQL JSONB for encrypted keys

            // Foreign Keys
            entity.HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.ReplyToMessage)
                .WithMany()
                .HasForeignKey(m => m.ReplyToMessageId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(m => m.ConversationId);
            entity.HasIndex(m => m.SenderId);
            entity.HasIndex(m => new { m.ConversationId, m.CreatedAt })
                .IsDescending(false, true); // For pagination
            entity.HasIndex(m => m.CreatedAt);
            entity.HasIndex(m => new { m.ConversationId, m.IsDeleted })
                .HasFilter("is_deleted = false"); // Active messages only
        });
    }
}
