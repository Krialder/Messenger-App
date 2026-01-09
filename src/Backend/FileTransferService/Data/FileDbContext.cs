using Microsoft.EntityFrameworkCore;

namespace FileTransferService.Data
{
    /// <summary>
    /// Database context for encrypted files
    /// </summary>
    public class FileDbContext : DbContext
    {
        public FileDbContext(DbContextOptions<FileDbContext> options) : base(options)
        {
        }

        public DbSet<FileMetadata> FileMetadata { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FileMetadata>(entity =>
            {
                entity.ToTable("file_metadata", "files");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.SenderId)
                    .HasColumnName("sender_id")
                    .IsRequired();

                entity.Property(e => e.RecipientId)
                    .HasColumnName("recipient_id")
                    .IsRequired();

                entity.Property(e => e.ConversationId)
                    .HasColumnName("conversation_id");

                entity.Property(e => e.OriginalFilename)
                    .HasColumnName("original_filename")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.FileSizeBytes)
                    .HasColumnName("file_size_bytes")
                    .IsRequired();

                entity.Property(e => e.ContentType)
                    .HasColumnName("content_type")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.StoragePath)
                    .HasColumnName("storage_path")
                    .IsRequired();

                entity.Property(e => e.EncryptedKey)
                    .HasColumnName("encrypted_key")
                    .IsRequired();

                entity.Property(e => e.Nonce)
                    .HasColumnName("nonce")
                    .IsRequired();

                entity.Property(e => e.UploadTimestamp)
                    .HasColumnName("upload_timestamp")
                    .HasDefaultValueSql("NOW()");

                entity.Property(e => e.DownloadCount)
                    .HasColumnName("download_count")
                    .HasDefaultValue(0);

                entity.Property(e => e.IsDeleted)
                    .HasColumnName("is_deleted")
                    .HasDefaultValue(false);

                entity.Property(e => e.DeletedAt)
                    .HasColumnName("deleted_at");

                entity.HasIndex(e => e.RecipientId)
                    .HasDatabaseName("idx_files_recipient");

                entity.HasIndex(e => e.ConversationId)
                    .HasDatabaseName("idx_files_conversation");

                entity.HasIndex(e => e.SenderId)
                    .HasDatabaseName("idx_files_sender");
            });
        }
    }

    /// <summary>
    /// Entity for encrypted file metadata
    /// </summary>
    public class FileMetadata
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid RecipientId { get; set; }
        public Guid? ConversationId { get; set; }
        public string OriginalFilename { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty;
        public byte[] EncryptedKey { get; set; } = Array.Empty<byte>();
        public byte[] Nonce { get; set; } = Array.Empty<byte>();
        public DateTime UploadTimestamp { get; set; }
        public int DownloadCount { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
