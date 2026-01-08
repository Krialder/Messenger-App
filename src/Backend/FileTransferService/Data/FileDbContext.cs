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

        public DbSet<EncryptedFile> EncryptedFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // PSEUDO: Configure EncryptedFile entity
            modelBuilder.Entity<EncryptedFile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.EncryptedContent).IsRequired();
                entity.Property(e => e.FileSize).IsRequired();
                entity.Property(e => e.SenderId).IsRequired();
                entity.Property(e => e.RecipientId).IsRequired();
                entity.Property(e => e.UploadedAt).IsRequired();
                entity.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

                // Indexes
                entity.HasIndex(e => e.SenderId);
                entity.HasIndex(e => e.RecipientId);
                entity.HasIndex(e => e.UploadedAt);
            });

            base.OnModelCreating(modelBuilder);
        }
    }

    /// <summary>
    /// Entity for encrypted files
    /// </summary>
    public class EncryptedFile
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string EncryptedContent { get; set; } = string.Empty;
        public string EncryptionMetadata { get; set; } = string.Empty;
        public Guid SenderId { get; set; }
        public Guid RecipientId { get; set; }
        public DateTime UploadedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
