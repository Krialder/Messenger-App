using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MessengerClient.Data
{
    public class LocalDbContext : DbContext
    {
        public DbSet<LocalMessage> Messages { get; set; }
        public DbSet<LocalConversation> Conversations { get; set; }
        public DbSet<LocalContact> Contacts { get; set; }
        public DbSet<LocalUserProfile> UserProfile { get; set; }
        public DbSet<LocalKeyPair> KeyPairs { get; set; }

        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LocalMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.SenderId).IsRequired();
                entity.Property(e => e.ConversationId).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
                entity.HasIndex(e => e.ConversationId);
                entity.HasIndex(e => e.Timestamp);
            });

            modelBuilder.Entity<LocalConversation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            modelBuilder.Entity<LocalContact>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.DisplayName).IsRequired();
                entity.HasIndex(e => e.UserId).IsUnique();
            });

            modelBuilder.Entity<LocalUserProfile>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Email).IsRequired();
            });

            modelBuilder.Entity<LocalKeyPair>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PublicKey).IsRequired();
                entity.Property(e => e.PrivateKey).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });
        }
    }

    public class LocalMessage
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? EncryptedContent { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsSent { get; set; }
        public bool IsDelivered { get; set; }
        public bool IsRead { get; set; }
        public string? FileId { get; set; }
        public string? FileName { get; set; }
    }

    public class LocalConversation
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = "DirectMessage";
        public string? Name { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public string? LastMessagePreview { get; set; }
        public int UnreadCount { get; set; }
    }

    public class LocalContact
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastSeen { get; set; }
        public byte[]? PublicKey { get; set; }
    }

    public class LocalUserProfile
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public byte[]? Salt { get; set; }
        public byte[]? PublicKey { get; set; }
        public bool MfaEnabled { get; set; }
    }

    public class LocalKeyPair
    {
        public Guid Id { get; set; }
        public byte[] PublicKey { get; set; } = Array.Empty<byte>();
        public byte[] PrivateKey { get; set; } = Array.Empty<byte>();
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
